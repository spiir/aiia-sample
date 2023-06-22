using System;
using System.Reflection;
using System.Threading;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Aiia.Sample.Extensions;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder UseKeyVault(this IWebHostBuilder builder)
    {
        return builder.ConfigureAppConfiguration((context, config) =>
        {
            var builtConfig = config.Build();
            if (builtConfig["KEY_VAULT_NAME"]
                .IsSet())
            {
                var secretClient = new SecretClient(new Uri($"https://{builtConfig["KEY_VAULT_NAME"]}.vault.azure.net/"),
                                                         new DefaultAzureCredential());
                config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
            }
        });
    }

    public static IWebHostBuilder UseSerilogElasticSearchIngest(this IWebHostBuilder builder)
    {
        return builder.UseSerilog((context, configuration) =>
        {
            configuration.Enrich.FromLogContext();
            configuration.Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
            configuration.Enrich.WithProperty("Type", "aiia-sample");
            configuration.Enrich.WithProperty("Culture", Thread.CurrentThread.CurrentCulture);
            configuration.Enrich.WithProperty("Version",
                Assembly.GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute
                    >()
                    .InformationalVersion);
            configuration.MinimumLevel.Information();
            configuration.MinimumLevel.Override("System", LogEventLevel.Information);
            configuration.MinimumLevel.Override("Microsoft", LogEventLevel.Information);

            var options = new SiteOptions();
            context.Configuration.Bind(options);

            if (!string.IsNullOrEmpty(options.ElasticSearch?.IngestToken))
                configuration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(options.ElasticSearch.IngestUrl))
                {
                    MinimumLogEventLevel =
                        LogEventLevel
                            .Debug,
                    ModifyConnectionSettings =
                        c =>
                            c.BasicAuthentication(options.ElasticSearch
                                    .IngestToken,
                                ""),
                    Period = TimeSpan
                        .FromMilliseconds(500)
                });

            if (context.HostingEnvironment.IsDevelopment())
                configuration.WriteTo.Console();
        });
    }
}