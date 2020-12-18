using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Aiia.Sample.Services
{
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
            return _optionsMonitor.CurrentValue.SendGrid?.ApiKey != null
                       ? SendSendgridEmail(destination, subject, emailHtml)
                       : SendConsoleEmail(destination, subject, emailHtml);
        }

        private async Task<bool> SendSendgridEmail(string destination, string subject, string emailHtml)
        {
            var sendGrid = _optionsMonitor.CurrentValue.SendGrid;
            var client = new SendGridClient(sendGrid.ApiKey);
            var from = new EmailAddress(sendGrid.EmailFrom, sendGrid.NameFrom);
            var to = new EmailAddress(destination);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, emailHtml);

            var res = await client.SendEmailAsync(msg);
            var success = ( int ) res.StatusCode >= 200 && ( int ) res.StatusCode <= 299;

            if (!success)
                _logger.LogError($"failed to send email via send grid, status code is: {res.StatusCode}",
                                 res.DeserializeResponseHeaders(res.Headers),
                                 res.DeserializeResponseBodyAsync(res.Body));

            return success;
        }

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}
