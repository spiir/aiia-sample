using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using ViiaSample.Extensions;

namespace ViiaSample
{
    public class Program
    {
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                          .UseStartup<Startup>()
                          .UseKeyVault()
                          .UseSentry(config =>
                                     {
                                         var environmentName =
                                             Environment
                                                 .GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                                         config.Environment = environmentName;
                                     })
                          .UseSerilogHumio();
        }

        public static async Task Main(string[] args)
        {
            await CreateWebHostBuilder(args).Build().RunAsync();
        }
    }
}
