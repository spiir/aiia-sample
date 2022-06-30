using System;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
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
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(
                        azureServiceTokenProvider
                            .KeyVaultTokenCallback));

                config.AddAzureKeyVault(
                    $"https://{builtConfig["KEY_VAULT_NAME"]}.vault.azure.net/",
                    keyVaultClient,
                    new DefaultKeyVaultSecretManager());
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

            if (options.ElasticSearch.IngestToken.IsSet() && options.ElasticSearch.IngestUrl.IsSet())
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