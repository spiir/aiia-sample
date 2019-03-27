using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDataSample.Data;
using MyDataSample.Services;

namespace MyDataSample.Controllers
{
    [Route("mydata")]
    [Authorize]
    public class MyDataController : Controller
    {
        private readonly IMyDataService _myDataService;
        private readonly ApplicationDbContext _dbContext;

        public MyDataController(IMyDataService myDataService, ApplicationDbContext dbContext)
        {
            _myDataService = myDataService;
            _dbContext = dbContext;
        }

        // Web hook for mydata to push data
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
            var myDataUrl = _myDataService.GetAuthUri(User);
            
            return Redirect(myDataUrl.ToString());
        }

        [HttpGet("callback")]
        public async Task<IActionResult> LoginCallback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest();
            }

            var tokenResponse = await _myDataService.ExchangeCodeForAccessToken(code);
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                return Unauthorized();
            }

            user.MyDataAccessToken = tokenResponse.AccessToken;
            user.MyDataTokenType = tokenResponse.TokenType;
            user.MyDataRefreshToken = tokenResponse.RefreshToken;
            user.MyDataAccessTokenExpires = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            
            return RedirectToAction("Accounts", "MyData");
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> Accounts()
        {
            var accounts = await _myDataService.GetUserAccounts(User);
            return View(accounts);
        }
        
        [HttpGet("transactions")]
        public async Task<IActionResult> Transactions([FromQuery] string accountId)
        {
            var transactions = await _myDataService.GetAccountTransactions(User, accountId);
            return View(transactions);
        }

        [HttpGet("mock")]
        public IActionResult MyDataAppMock()
        {
            return View();
        }
    }
}