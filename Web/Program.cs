using System.Threading.Tasks;
using Aiia.Sample.Extensions;
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
                builder.UseKestrel(options =>
                    {
                        options.AddServerHeader = false;
                    })
                    .UseStartup<Startup>()
                    .UseKeyVault()
                    .UseSerilogElasticSearchIngest();
            });
    }

    public static async Task Main(string[] args)
    {
        await CreateHostBuilder(args).Build().RunAsync();
    }
}