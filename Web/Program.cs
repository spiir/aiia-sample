using System.Threading.Tasks;
using Aiia.Sample.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Aiia.Sample;

public class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(builder =>
            {
                builder
                    .UseStartup<Startup>()
                    .UseKeyVault()
                    .UseSerilogElasticSearchIngest();
                
                builder.ConfigureKestrel(options =>
                {
                    options.AddServerHeader = false;
                });
            });
    }

    public static async Task Main(string[] args)
    {
        await CreateHostBuilder(args).Build().RunAsync();
    }
}