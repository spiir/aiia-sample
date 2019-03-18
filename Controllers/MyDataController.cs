using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDataSample.Services;

namespace MyDataSample.Controllers
{
    [Route("mydata")]
    [Authorize]
    public class MyDataController : Controller
    {
        private readonly IMyDataService _myDataService;

        public MyDataController(IMyDataService myDataService)
        {
            _myDataService = myDataService;
        }

        // Web hook for mydata to push data
        [HttpPost("callback")]
        [AllowAnonymous]
        public IActionResult Callback()
        {
            // Store whatever comes here
            return Ok("Thanks for data.");
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login()
        {
            var myDataUrl = await _myDataService.GetAuthUri(User);

            return Redirect(myDataUrl.ToString());
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> Accounts()
        {
            var accounts = await _myDataService.GetUserAccounts(User);
            return View(accounts);
        }
        
        [HttpGet("accounts/{accountId}/transactions")]
        public async Task<IActionResult> Transactions(string accountId)
        {
            var transactions = await _myDataService.GetAccountTransactions(accountId);
            return View(transactions);
        }
    }
}