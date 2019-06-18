using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ViiaSample.Data;
using ViiaSample.Models;
using ViiaSample.Services;

namespace ViiaSample.Controllers
{
    [Route("viia")]
    [Authorize]
    public class ViiaController : Controller
    {
        private readonly IViiaService _viiaService;
        private readonly ApplicationDbContext _dbContext;

        public ViiaController(IViiaService viiaService, ApplicationDbContext dbContext)
        {
            _viiaService = viiaService;
            _dbContext = dbContext;
        }

        // Web hook for Viia to push data
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> DataCallback([FromBody] JObject payload)
        {
            await _viiaService.ProcessWebHookPayload(payload);
            return Ok();
        }

        [HttpPost("update")]
        public async Task<IActionResult> RequestDataUpdate()
        {
            var dataUpdateResponse = await _viiaService.InitiateDataUpdate(User);
            return Ok(new
            {
                authUrl = dataUpdateResponse.Status == UpdateStatus.AllQueued
                    ? string.Empty
                    : dataUpdateResponse.AuthUrl
            });
        }


        [HttpGet("login")]
        public IActionResult Login()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            var ViiaUrl = _viiaService.GetAuthUri(User, user?.Email);

            return Redirect(ViiaUrl.ToString());
        }

        [HttpGet("callback")]
        public async Task<IActionResult> LoginCallback([FromQuery] string code, [FromQuery] string consentId)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(consentId))
            {
                return BadRequest();
            }

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

            return View("GenericViewWithPostMessageOnLoad", Request.QueryString.Value);
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> Accounts()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user?.ViiaAccessToken == null || user.ViiaAccessTokenExpires < DateTimeOffset.UtcNow)
            {
                return View(new AccountViewModel
                {
                    AccountsGroupedByProvider = null,
                    ViiaConnectUrl = _viiaService.GetAuthUri(User, user?.Email).ToString(),
                    EmailEnabled = user?.EmailEnabled ?? false
                });
            }

            var accounts = await _viiaService.GetUserAccounts(User);
            var groupedAccounts = accounts.ToLookup(x => x.Provider?.Id, x => x);

            var model = new AccountViewModel
            {
                AccountsGroupedByProvider = groupedAccounts,
                ViiaConnectUrl = _viiaService.GetAuthUri(User, user.Email).ToString(),
                JwtToken = new JwtSecurityTokenHandler().ReadJwtToken(user.ViiaAccessToken),
                RefreshToken = new JwtSecurityTokenHandler().ReadJwtToken(user.ViiaRefreshToken),
                EmailEnabled = user.EmailEnabled
            };
            return View(model);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> Transactions([FromQuery] string accountId)
        {
            var transactions = await _viiaService.GetAccountTransactions(User, accountId);
            return View(transactions);
        }

        [HttpPost("toggle-email")]
        public async Task<IActionResult> DisconnectFromViia()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                return Ok(new {});
            }

            user.EmailEnabled = !user.EmailEnabled;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return Ok(new {updatedStatus = user.EmailEnabled});
        }
    }
}