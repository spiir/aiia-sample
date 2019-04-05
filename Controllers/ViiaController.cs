using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViiaSample.Data;
using ViiaSample.Services;

namespace ViiaSample.Controllers
{
    [Route("viia")]
    [Authorize]
    public class ViiaController : Controller
    {
        private readonly IViiaService _ViiaService;
        private readonly ApplicationDbContext _dbContext;

        public ViiaController(IViiaService ViiaService, ApplicationDbContext dbContext)
        {
            _ViiaService = ViiaService;
            _dbContext = dbContext;
        }

        // Web hook for Viia to push data
        [HttpPost("data")]
        [AllowAnonymous]
        public IActionResult DataCallback()
        {
            // Store whatever comes here
            return Ok("Thanks for data.");
        }
        
        [HttpGet("login")]
        public async Task<IActionResult> Login()
        {
            var ViiaUrl = _ViiaService.GetAuthUri(User);
            
            return Redirect(ViiaUrl.ToString());
        }

        [HttpGet("callback")]
        public async Task<IActionResult> LoginCallback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest();
            }

            var tokenResponse = await _ViiaService.ExchangeCodeForAccessToken(code);
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
            
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            
            return RedirectToAction("Accounts", "Viia");
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> Accounts()
        {
            var accounts = await _ViiaService.GetUserAccounts(User);
            return View(accounts);
        }
        
        [HttpGet("transactions")]
        public async Task<IActionResult> Transactions([FromQuery] string accountId)
        {
            var transactions = await _ViiaService.GetAccountTransactions(User, accountId);
            return View(transactions);
        }
    }
}