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
            var myDataUrl = await _myDataService.GetAuthUri(User);

            return Redirect(myDataUrl.ToString());
        }

        [HttpGet("callback")]
        public IActionResult LoginCallback()
        {
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
            var transactions = await _myDataService.GetAccountTransactions(accountId);
            return View(transactions);
        }

        [HttpGet("mock")]
        public IActionResult MyDataAppMock()
        {
            return View();
        }
    }
}