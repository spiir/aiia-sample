using System;
using System.Threading.Tasks;
using Aiia.Sample.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Aiia.Sample
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(options =>
                                                                            {
                                                                                options.UseStartup<Startup>().UseKeyVault()
                                                                                       .UseSentry(config =>
                                                                                                  {
                                                                                                      var environmentName =
                                                                                                          Environment
                                                                                                              .GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                                                                                                      config.Environment = environmentName;
                                                                                                  })
                                                                                       .UseSerilogHumio();
                                                                            });
        }

        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }
    }
}
