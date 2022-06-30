using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Aiia.Sample.Services;

public interface IEmailService
{
    Task<bool> SendWebhookEmail(string destinationEmail, string fullJson);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IOptionsMonitor<SiteOptions> _optionsMonitor;

    public EmailService(IOptionsMonitor<SiteOptions> optionsMonitor, ILogger<EmailService> logger)
    {
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public async Task<bool> SendWebhookEmail(string destinationEmail, string fullJson)
    {
        var emailHtml =
            $"<p>Here is the webhook we received from Aiia:</p><br /><pre><code>\n{FormatJson(fullJson)}\n</code></pre>";
        var result = await SendEmail(destinationEmail, "Your bank data got updated", emailHtml);
        _logger.LogInformation($"Send webhook email. Success: {result}");
        return result;
    }

    private Task<bool> SendConsoleEmail(string destination, string subject, string emailHtml)
    {
        _logger.LogInformation(
            $"\n\n========= EMAIL =========\n\nTO:<{destination}\nSUBJECT: {subject}\n\n{emailHtml}");
        return Task.FromResult(true);
    }

    private Task<bool> SendEmail(string destination, string subject, string emailHtml)
    {
        return SendConsoleEmail(destination, subject, emailHtml);
    }


    private static string FormatJson(string json)
    {
        dynamic parsedJson = JsonConvert.DeserializeObject(json);
        return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
    }
}