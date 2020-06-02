using System;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViiaSample.Data;
using ViiaSample.Models;
using ViiaSample.Models.Viia;
using ViiaSample.Services;

namespace ViiaSample.Controllers
{
    [Route("viia")]
    [Authorize]
    public class ViiaController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IViiaService _viiaService;

        public ViiaController(IViiaService viiaService, ApplicationDbContext dbContext)
        {
            _viiaService = viiaService;
            _dbContext = dbContext;
        }

        // Web hook for Viia to push data to
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> DataCallback()
        {
            await _viiaService.ProcessWebHookPayload(Request);
            return Ok();
        }

        [HttpGet("data/{consentId}")]
        [AllowAnonymous]
        public IActionResult DataUpdateCallback()
        {
            return View("GenericViewWithPostMessageOnLoad", new CallbackViewModel {AutomaticallyFinish = true});
        }

        // Toggles email notifications for webhook, might be interesting to check how/when/what Viia sends in webhooks, but gets a bit annoying in the long run
        [HttpPost("toggle-email")]
        public async Task<IActionResult> DisconnectFromViia()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                return Ok(new { });
            }

            user.EmailEnabled = !user.EmailEnabled;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return Ok(new {updatedStatus = user.EmailEnabled});
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            var viiaUrl = _viiaService.GetAuthUri(user?.Email);

            return Redirect(viiaUrl.ToString());
        }

        [HttpGet("callback")]
        public async Task<IActionResult> LoginCallback([FromQuery] string code, [FromQuery] string consentId)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(consentId))
                return View("GenericViewWithPostMessageOnLoad", new CallbackViewModel {IsError = true});

            // Immediately exchange received code for an access token, since code has a short lifespan
            var tokenResponse = await _viiaService.ExchangeCodeForAccessToken(code);
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                return Unauthorized();
            }

            user.ViiaAccessToken = tokenResponse.AccessToken;
            user.ViiaTokenType = tokenResponse.TokenType;
            user.ViiaRefreshToken = tokenResponse.RefreshToken;
            user.ViiaAccessTokenExpires = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            user.ViiaConsentId = consentId;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return View("GenericViewWithPostMessageOnLoad",
                new CallbackViewModel
                    {Query = Request.QueryString.Value, AutomaticallyFinish = false, IsError = false});
        }

        [HttpPost("update")]
        public async Task<IActionResult> RequestDataUpdate()
        {
            var dataUpdateResponse = await _viiaService.InitiateDataUpdate(User);

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