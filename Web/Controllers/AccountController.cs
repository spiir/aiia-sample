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
using ViiaSample.Services;

namespace ViiaSample.Controllers
{
    [Route("viia/accounts")]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IViiaService _viiaService;

        public AccountController(ApplicationDbContext dbContext, IViiaService viiaService)
        {
            _dbContext = dbContext;
            _viiaService = viiaService;
        }

        [HttpGet("")]
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
                    Providers = providers,
                    Email = user?.Email
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
                Providers = providers,
                ConsentId = user.ViiaConsentId,
                Email = user.Email
            };
            return View(model);
        }
        
        [HttpPost("{accountId}/transactions/query")]
        public async Task<IActionResult> FetchTransactions(string accountId,
            [FromBody] TransactionQueryRequestViewModel body)
        {
            var transactions = await _viiaService.GetAccountTransactions(User, accountId, body);
            return Ok(new TransactionsViewModel(transactions.Transactions, transactions.PagingToken,
                body.IncludeDeleted));
        }
        
        
        [HttpGet("{accountId}/transactions")]
        public async Task<IActionResult> Transactions(string accountId)
        {
            var transactions = await _viiaService.GetAccountTransactions(User, accountId);
            return View(new TransactionsViewModel(transactions.Transactions, transactions.PagingToken, false));
        }
    }
}