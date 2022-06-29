using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Aiia.Sample.Data;
using Aiia.Sample.Models;
using Aiia.Sample.Models.Aiia;
using Aiia.Sample.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aiia.Sample.Controllers
{
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

        // Toggles email notifications for webhook, might be interesting to check how/when/what Aiia sends in webhooks, but gets a bit annoying in the long run
        [HttpPost("toggle-email")]
        public async Task<IActionResult> DisconnectFromAiia()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null) return Ok(new { });

            user.EmailEnabled = !user.EmailEnabled;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return Ok(new { updatedStatus = user.EmailEnabled });
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
            var tokenResponse = await _aiiaService.ExchangeCodeForAccessToken(code);
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null) return Unauthorized();

            user.AiiaAccessToken = tokenResponse.AccessToken;
            user.AiiaTokenType = tokenResponse.TokenType;
            user.AiiaRefreshToken = tokenResponse.RefreshToken;
            user.AiiaAccessTokenExpires = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            user.AiiaConsentId = consentId;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

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
}