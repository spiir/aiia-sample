using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ViiaSample.Data;
using ViiaSample.Services;

namespace ViiaSample
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            UpdateDatabase(app);
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();
            app.UseEndpoints(endpoints =>
                             {
                                 endpoints.MapControllerRoute(
                                                              "default",
                                                              "{controller=Home}/{action=Index}/{id?}");
                                 endpoints.MapControllers();
                                 endpoints.MapRazorPages();
                             });
        }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                                                            options.UseSqlite(
                                                                              Configuration.GetConnectionString("DefaultConnection")));
            ConfigureStandardServices(services);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                                                            options.UseSqlServer(
                                                                                 Configuration.GetConnectionString("DefaultConnection")));
            ConfigureStandardServices(services);
        }

        private void ConfigureStandardServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
                                                    {
                                                        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                                                        options.CheckConsentNeeded = context => true;
                                                        options.MinimumSameSitePolicy = SameSiteMode.None;
                                                    });

            services.Configure<SiteOptions>(Configuration);

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                                                                {
                                                                    options.Password.RequireDigit = false;
                                                                    options.Password.RequireLowercase = false;
                                                                    options.Password.RequireNonAlphanumeric = false;
                                                                    options.Password.RequireUppercase = false;
                                                                    options.Password.RequiredLength = 4;
                                                                    options.Password.RequiredUniqueChars = 1;

                                                                    options.User.RequireUniqueEmail = true;
                                                                })
                    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddScoped<IViiaService, ViiaService>();
            services.AddRazorPages();
            services.AddControllers();
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                                         .GetRequiredService<IServiceScopeFactory>()
                                         .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    if (context.Database.IsSqlServer())
                    {
                        context.Database.Migrate();
                    }
                    else
                    {
                        // EnsureCreated() bypasses migrations and creates schema for model
                        // Should be used only for prototyping/testing.
                        context.Database.EnsureCreated();
                    }
                }
            }
        }
    }
}
