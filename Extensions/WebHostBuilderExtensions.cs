using System;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace ViiaSample.Extensions
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseSerilogHumio(this IWebHostBuilder builder)
        {
            return builder.UseSerilog((context, configuration) =>
                                      {
                                          configuration.Enrich.FromLogContext();
                                          configuration.Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
                                          configuration.Enrich.WithProperty("Type", "Viia Sample");
                                          configuration.Enrich.WithProperty("Culture", Thread.CurrentThread.CurrentCulture);
                                          configuration.Enrich.WithProperty("Version",
                                                                            Assembly.GetEntryAssembly()
                                                                                    .GetCustomAttribute<AssemblyInformationalVersionAttribute
                                                                                    >()
                                                                                    .InformationalVersion);
                                          configuration.MinimumLevel.Information();
                                          configuration.MinimumLevel.Override("System", LogEventLevel.Information);
                                          configuration.MinimumLevel.Override("Microsoft", LogEventLevel.Information);

                                          var options = new HumioOptions();
                                          context.Configuration.GetSection("Humio")
                                                 .Bind(options);

                                          if (options.IngestToken.IsSet() && options.IngestUrl.IsSet())
                                              configuration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(options.IngestUrl))
                                                                                  {
                                                                                      MinimumLogEventLevel =
                                                                                          LogEventLevel
                                                                                              .Debug,
                                                                                      ModifyConnectionSettings =
                                                                                          c =>
                                                                                              c.BasicAuthentication(options
                                                                                                                        .IngestToken,
                                                                                                                    ""),
                                                                                      Period = TimeSpan
                                                                                          .FromMilliseconds(500)
                                                                                  });

                                          if (context.HostingEnvironment.IsDevelopment())
                                              configuration.WriteTo.Console(outputTemplate:
                                                                            "[{Timestamp:HH:mm:ss} {Level:u3}] {Properties:j} {Message:lj}{NewLine}{Exception}");
                                      });
        }
    }
}