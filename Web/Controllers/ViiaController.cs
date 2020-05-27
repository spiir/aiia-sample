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

        [HttpGet("accounts")]
        public async Task<IActionResult> Accounts()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);

            var providers = await _viiaService.GetProviders();
            providers = providers
                        .OrderBy(x => x.CountryCode)
                        .ThenBy(y => y.Name)
                        .ToImmutableList();

            // If user hasn't connected to Viia or his access token is expired, show empty accounts page
            if (user?.ViiaAccessToken == null || user.ViiaAccessTokenExpires < DateTimeOffset.UtcNow)
            {
                return View(new AccountsViewModel
                            {
                                AccountsGroupedByProvider = null,
                                ViiaConnectUrl = _viiaService.GetAuthUri(user?.Email).ToString(),
                                ViiaOneTimeConnectUrl = _viiaService.GetAuthUri(null, true).ToString(),
                                EmailEnabled = user?.EmailEnabled ?? false,
                                Providers = providers
                            });
            }

            var accounts = await _viiaService.GetUserAccounts(User);
            var groupedAccounts = accounts.ToLookup(x => x.AccountProvider?.Id, x => x);

            var model = new AccountsViewModel
                        {
                            AccountsGroupedByProvider = groupedAccounts,
                            ViiaConnectUrl = _viiaService.GetAuthUri(user.Email).ToString(),
                            ViiaOneTimeConnectUrl = _viiaService.GetAuthUri(null, true).ToString(),
                            JwtToken = new JwtSecurityTokenHandler().ReadJwtToken(user.ViiaAccessToken),
                            RefreshToken = new JwtSecurityTokenHandler().ReadJwtToken(user.ViiaRefreshToken),
                            EmailEnabled = user.EmailEnabled,
                            Providers = providers
                        };
            return View(model);
        }

        [HttpGet("payments")]
        public async Task<IActionResult> Payments()
        {
            IImmutableList<Account> accounts = ImmutableList.Create<Account>();
            try
            {
                accounts = await _viiaService.GetUserAccounts(User);
            }
            catch (ViiaClientException e)
            {
                // TODO
            }
            return View(new PaymentsViewModel
            {
                Accounts = accounts
            });
        }

        [HttpPost("payments")]
        public async Task<ActionResult<CreatePaymentResultViewModel>> CreatePayment([FromBody] CreatePaymentRequestViewModel body)
        {
            var result = new CreatePaymentResultViewModel();
            try
            {
                var createPaymentResult = await _viiaService.CreatePayment(User, body);
                result.PaymentId = createPaymentResult.PaymentId;
                result.PaymentId = createPaymentResult.PaymentUrl;
            }
            catch (ViiaClientException e)
            {
                result.ErrorDescription = e.Message;
            }
            return Ok(result);
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
            return View("GenericViewWithPostMessageOnLoad", new CallbackViewModel { AutomaticallyFinish = true });
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
            return Ok(new { updatedStatus = user.EmailEnabled });
        }


        [HttpPost("accounts/{accountId}/transactions/query")]
        public async Task<IActionResult> FetchTransactions(string accountId, [FromBody] TransactionQueryRequestViewModel body)
        {
            var transactions = await _viiaService.GetAccountTransactions(User, accountId, body);
            return Ok(new TransactionsViewModel(transactions.Transactions, transactions.PagingToken, body.IncludeDeleted));
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
                return View("GenericViewWithPostMessageOnLoad", new CallbackViewModel { IsError = true });

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
                        { Query = Request.QueryString.Value, AutomaticallyFinish = false, IsError = false });
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

        [HttpGet("accounts/{accountId}/transactions")]
        public async Task<IActionResult> Transactions(string accountId)
        {
            var transactions = await _viiaService.GetAccountTransactions(User, accountId);
            return View(new TransactionsViewModel(transactions.Transactions, transactions.PagingToken, false));
        }
    }
}
