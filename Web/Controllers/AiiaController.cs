using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Aiia.Sample.AiiaClient;
using Aiia.Sample.AiiaClient.Models;
using Aiia.Sample.Data;
using Aiia.Sample.Models;
using Aiia.Sample.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aiia.Sample.Controllers;

[Route("aiia")]
[Authorize]
public class AiiaController : Controller
{
    private readonly IAiiaService _aiiaService;
    private readonly ApplicationDbContext _dbContext;

    public AiiaController(IAiiaService aiiaService, ApplicationDbContext dbContext)
    {
        _aiiaService = aiiaService;
        _dbContext = dbContext;
    }

    // Web hook for Aiia to push data to
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> DataCallback()
    {
        await _aiiaService.ProcessWebHookPayload(Request);
        return Ok();
    }

    [HttpGet("data/{consentId}")]
    [AllowAnonymous]
    public IActionResult DataUpdateCallback()
    {
        return View("GenericViewWithPostMessageOnLoad", new CallbackViewModel { AutomaticallyFinish = true });
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
        var aiiaUrl = _aiiaService.GetAuthUri(user?.Email);

        return Redirect(aiiaUrl.ToString());
    }

    [HttpGet("callback")]
    public async Task<IActionResult> LoginCallback([FromQuery] string code, [FromQuery] string consentId)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(consentId))
            return View("GenericViewWithPostMessageOnLoad", new CallbackViewModel { IsError = true });

        // Immediately exchange received code for an access token, since code has a short lifespan
        await _aiiaService.ExchangeCodeForAccessToken(User, code, consentId);

        return View("GenericViewWithPostMessageOnLoad",
            new CallbackViewModel
                { Query = Request.QueryString.Value, AutomaticallyFinish = false, IsError = false });
    }

    [HttpPost("update")]
    public async Task<IActionResult> RequestDataUpdate()
    {
        var dataUpdateResponse = await _aiiaService.InitiateDataUpdate(User);

        // If status is `AllQueued`, it means that all connections didn't need a supervised login and were queued successfully
        // Otherwise, a supervised login is needed by the user using the `AuthUrl` received in the response
        return Ok(new
        {
            authUrl = dataUpdateResponse.Status == InitiateDataUpdateResponse.UpdateStatus.AllQueued
                ? string.Empty
                : dataUpdateResponse.AuthUrl
        });
    }
}