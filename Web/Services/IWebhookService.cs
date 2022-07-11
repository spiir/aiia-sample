using System.Collections.Generic;
using System.Threading.Tasks;
using Aiia.Sample.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Aiia.Sample.AiiaClient;

public interface IWebhookService
{
    Task<List<Webhook>> GetAllWebhooks(ApplicationUser applicationUser);
    Task ProcessWebHookPayload(string timestamp, string eventId, string eventType, string aiiaSignature, string payloadString);
}