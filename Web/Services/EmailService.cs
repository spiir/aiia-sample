using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ViiaSample.Services
{
    public interface IEmailService
    {
        Task<bool> SendDataUpdateEmail(string destinationEmail, string fullJson);
        Task<bool> SendConnectionUpdateRequiredEmail(string destinationEmail, string fullJson);
        Task<bool> SendConsentUpdateRequiredEmail(string destinationEmail, string fullJson);
        Task<bool> SendConsentRevokedEmail(string destinationEmail, string fullJson);
        Task<bool> SendUnknownWebHookEmail(string destinationEmail, string fullJson);
        Task SendConnectionRemovedEmail(string userEmail, string toString);
        Task SendSyncProgressUpdateEmail(string userEmail, string toString);
    }

    public class EmailService : IEmailService
    {
        private readonly IOptionsMonitor<SiteOptions> _optionsMonitor;
        private readonly ILogger<EmailService> _logger;
        
        public EmailService(IOptionsMonitor<SiteOptions> optionsMonitor, ILogger<EmailService> logger)
        {
            _optionsMonitor = optionsMonitor;
            _logger = logger;
        }

        public Task<bool> SendDataUpdateEmail(string destinationEmail, string fullJson)
        {
            var emailHtml =
                $"<p>We have updated your bank data</p><br /><p>Here is the webhook we received from Viia:</p><br /><pre><code>\n{FormatJson(fullJson)}\n</code></pre>";
            return SendEmail(destinationEmail, "Your bank data got updated", emailHtml);
        }

        public Task<bool> SendConnectionUpdateRequiredEmail(string destinationEmail, string fullJson)
        {
            var emailHtml =
                $"<p>It seems that you need to login again for us to fetch updated data from your bank account.</p><br /><p>Here is the webhook we received from Viia:</p><br /><pre><code>\n{FormatJson(fullJson)}\n</code></pre>";
            return SendEmail(destinationEmail, "Your bank data got updated", emailHtml);
        }
        
        public Task<bool> SendConsentUpdateRequiredEmail(string destinationEmail, string fullJson)
        {
            var emailHtml =
                $"<p>Viia has updated terms and conditions that you need to accept for us to fetch updated data from your bank account.</p><br /><p>Here is the webhook we received from Viia:</p><br /><pre><code>\n{FormatJson(fullJson)}\n</code></pre>";
            return SendEmail(destinationEmail, "Your bank data got updated", emailHtml);
        }
        public Task<bool> SendConsentRevokedEmail(string destinationEmail, string fullJson)
        {
            var emailHtml =
                $"<p>It seems that you revoked consent for us to access your bank account data, this means that we won't be able to show it anymore.</p><br /><p>Here is the webhook we received from Viia:</p><br /><pre><code>\n{FormatJson(fullJson)}\n</code></pre>";
            return SendEmail(destinationEmail, "Your bank data got updated", emailHtml);
        }

        public Task<bool> SendUnknownWebHookEmail(string destinationEmail, string fullJson)
        {
            var emailHtml =
                $"<p>We received unknown webhook for you.</p><br /><p>Here is the webhook we received from Viia:</p><br /><pre><code>\n{FormatJson(fullJson)}\n</code></pre>";
            return SendEmail(destinationEmail, "Unknown webhook", emailHtml); 
        }

        public Task SendConnectionRemovedEmail(string userEmail, string toString)
        {
            throw new System.NotImplementedException();
        }

        public Task SendSyncProgressUpdateEmail(string userEmail, string toString)
        {
            throw new System.NotImplementedException();
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

        private Task<bool> SendConsoleEmail(string destination, string subject, string emailHtml)
        {
            _logger.LogInformation($"\n\n========= EMAIL =========\n\nTO:<{destination}\nSUBJECT: {subject}\n\n{emailHtml}");
            return Task.FromResult(true);
        }

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}