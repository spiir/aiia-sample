using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Aiia.Sample.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aiia.Sample.AiiaClient;

class WebhookService : IWebhookService
{
    private readonly AiiaHttpClient _aiiaHttpClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<WebhookService> _logger;
    private readonly IOptionsMonitor<SiteOptions> _options;

    public WebhookService(IOptionsMonitor<SiteOptions> options,
        ILogger<WebhookService> logger,
        ApplicationDbContext dbContext)
    {
        _options = options;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task ProcessWebHookPayload(string timestamp, string eventId, string eventType, string aiiaSignature, string payloadString)
    {

        _logger.LogInformation("Received webhook: {PayloadString}", payloadString);
        // `X-Aiia-Signature` is provided optionally if client has configured `WebhookSecret` and is used to verify that webhook was sent by Aiia
        if (!VerifySignature(timestamp, eventId, eventType, aiiaSignature, payloadString))
        {
            _logger.LogWarning("Failed to verify webhook signature");
            return;
        }

        var payload = JObject.Parse(payloadString);
        var data = payload[payload.Properties().First().Name];

        if (data == null)
        {
            _logger.LogInformation("Webhook data not parsed");
            return;
        }

        var consentId = string.IsNullOrEmpty(data["consentId"]?.Value<string>())
            ? string.Empty
            : data["consentId"].Value<string>();

        var user = _dbContext.Users.FirstOrDefault(x => x.AiiaConsentId == consentId);
        if (user == null)
        {
            _logger.LogInformation("No user found with consent {ConsentId}", consentId);
            // User probably revoked consent
            return;
        }

        // we store native types in the database if possible
        long.TryParse(timestamp, out var timestampAsInt);
        Guid.TryParse(eventId, out var eventIdAsGuid);
        
        await SaveWebhookToDatabase(user, timestampAsInt, eventIdAsGuid, eventType, aiiaSignature, payload);
    }

    public Task<List<Webhook>> GetAllWebhooks(ApplicationUser applicationUser)
    {
        return _dbContext.Webhooks.Where(w => w.User == applicationUser).OrderByDescending(w=>w.Id).ToListAsync();
    }

    private async Task SaveWebhookToDatabase(ApplicationUser user, long timestamp, Guid eventId, string eventType, string aiiaSignature, JObject payload)
    {
        
        // 1. Clenup old webhooks
        //    - keep only the latest 100 webhooks for the user (limit the storage used by each user)
        var webhooksToRemove = (await _dbContext.Webhooks.Where(w => w.User == user)
            .OrderByDescending(w=>w.Id)
            .Skip(100)
            .ToListAsync())
            .ToHashSet(); 
        
        //    - remove webhooks older than 1 day for all users to keep the db size small and fast.
        var yesterday = DateTimeOffset.UtcNow.AddDays(-1).Ticks;
        var oldWebhooks = await _dbContext.Webhooks.Where(w => w.ReceivedAtTimestamp < yesterday).ToListAsync();
        webhooksToRemove.UnionWith(oldWebhooks);
        
        _dbContext.Webhooks.RemoveRange(webhooksToRemove);
        
        // 2. Add the new webhook
        _dbContext.Webhooks.Add(new Webhook()
            { DataAsJson = payload.ToString(Formatting.None), ReceivedAtTimestamp = DateTime.UtcNow.Ticks, User = user , EventType = eventType, Timestamp = timestamp, EventId = eventId, Signature = aiiaSignature});

        // 3. Save
        await _dbContext.SaveChangesAsync();
    }

    // Aiia calculates same HMAC hash using the secret only known by the client and Aiia
    // If HMAC hashes doesn't mach, it means that the webhook was not sent by Aiia

    public bool VerifySignature(string timestamp, string eventId, string eventType, string aiiaSignature, string payload)
    {
        if (string.IsNullOrWhiteSpace(aiiaSignature))
            return true;

        if (string.IsNullOrWhiteSpace(_options.CurrentValue.Aiia.WebHookSecret))
            return true;

        var generatedSignature = GenerateSignature(timestamp, eventId, eventType, payload, _options.CurrentValue.Aiia.WebHookSecret);

        if (generatedSignature != aiiaSignature)
        {
            _logger.LogWarning(
                $"Webhook signatures didn't match. Received:\n{aiiaSignature}\nGenerated: {generatedSignature}");
            return false;
        }

        return true;
    }

    string GenerateSignature(string timestamp, string eventId, string eventType, string body, string secret)
    {
        var textBytes = Encoding.UTF8.GetBytes($"{timestamp}|{eventId}|{eventType}|{body}");
        var keyBytes = Encoding.UTF8.GetBytes(secret);

        byte[] hashBytes;
        using (var hasher = new HMACSHA256(keyBytes))
        {
            hashBytes = hasher.ComputeHash(textBytes);
        }
        // Convert hash from "AA-BB-CC-..." to "aabbcc..."
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}