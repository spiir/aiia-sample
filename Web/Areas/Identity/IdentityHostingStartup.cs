using Microsoft.AspNetCore.Hosting;
using ViiaSample.Areas.Identity;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]

namespace ViiaSample.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => { });
        }
    }
}
