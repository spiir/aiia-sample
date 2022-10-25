using Aiia.Sample.AiiaClient;
using Aiia.Sample.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Aiia.Sample;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseRouting();

        if (!env.IsDevelopment())
        {
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
        JsonConvert.DefaultSettings = () =>
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            settings.Converters.Add(new StringEnumConverter());
            return settings;
        };
        
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
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 9;
                options.Password.RequiredUniqueChars = 1;

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IAiiaService, AiiaService>();
        services.AddHttpClient<IAiiaHttpClient, AiiaHttpClient>();
        services.AddSingleton<AiiaApi, AiiaApi>();
        services.AddRazorPages().AddNewtonsoftJson();
        services.AddControllers().AddNewtonsoftJson();
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
                    context.Database.Migrate();
                else
                    // EnsureCreated() bypasses migrations and creates schema for model
                    // Should be used only for prototyping/testing.
                    context.Database.EnsureCreated();
            }
        }
    }
}