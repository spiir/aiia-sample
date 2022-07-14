using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Aiia.Sample.AiiaClient;
using Aiia.Sample.Data;
using Aiia.Sample.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aiia.Sample.Controllers;

[Route("aiia/")]
[Authorize]
public class WebhookController: Controller
{
    private readonly IWebhookService _webhookService;
    private readonly ApplicationDbContext _dbContext;

    public WebhookController(IWebhookService webhookService, ApplicationDbContext dbContext)
    {
        _webhookService = webhookService;
        _dbContext = dbContext;
    }
    
    // Web hook for Aiia to push data to
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> DataCallback()
    {
        var payloadString = await ReadRequestBody(Request.Body);
        var aiiaSignature = Request.Headers["X-Aiia-Signature"];
        var timestamp = Request.Headers["X-Aiia-TimeStamp"];
        var eventId = Request.Headers["X-Aiia-EventId"];
        var eventType = Request.Headers["X-Aiia-Event"];
        
        await _webhookService.ProcessWebHookPayload(timestamp,  eventId, eventType, aiiaSignature, payloadString);
        return Ok();
    }

    [HttpGet("webhook/view")]
    public async Task<IActionResult> ViewWebhooks()
    {
        var user = await User.GetCurrentUser(_dbContext);
        var webhooks = await _webhookService.GetAllWebhooks(user);
        return View(new ViewWebhooksViewModel
        {
            Webhooks = webhooks
        });
    }
    private async Task<string> ReadRequestBody(Stream bodyStream)
    {
        string documentContents;
        using (bodyStream)
        {
            using (var readStream = new StreamReader(bodyStream, Encoding.UTF8))
            {
                documentContents = await readStream.ReadToEndAsync();
            }
        }

        return documentContents;
    }

}

public class ViewWebhooksViewModel
{
    public List<Webhook> Webhooks { get; set; }
}
