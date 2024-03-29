using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Aiia.Sample.AiiaClient;
using Aiia.Sample.Data;
using Aiia.Sample.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aiia.Sample.Controllers;

[Route("aiia/accounts")]
[Authorize]
public class AccountController : Controller
{
    private readonly IAiiaService _aiiaService;
    private readonly ApplicationDbContext _dbContext;

    public AccountController(ApplicationDbContext dbContext, IAiiaService aiiaService)
    {
        _dbContext = dbContext;
        _aiiaService = aiiaService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Accounts()
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);

        var providers = await _aiiaService.GetProviders();
        providers = providers
            .OrderBy(x => x.CountryCode)
            .ThenBy(y => y.Name)
            .ToImmutableList();

        // If user hasn't connected to Aiia then show an empty accounts page
        if (user?.AiiaAccessToken == null)
            return View(new AccountsViewModel
            {
                AccountsGroupedByProvider = null,
                AiiaConnectUrl = _aiiaService.GetAuthUri(user?.Email).ToString(),
                Providers = providers,
                Email = user?.Email,
                ConsentId = user?.AiiaConsentId
            });

        try
        {
            var accounts = await _aiiaService.GetAccounts(User);
            var groupedAccounts = accounts.ToLookup(x => x.AccountProvider?.Id, x => x);
            var allAccountsSelected = await _aiiaService.AllAccountsSelected(User);

            var model = new AccountsViewModel
            {
                AccountsGroupedByProvider = groupedAccounts,
                AiiaConnectUrl = _aiiaService.GetAuthUri(user.Email).ToString(),
                JwtToken = new JwtSecurityTokenHandler().ReadJwtToken(user.AiiaAccessToken),
                RefreshToken = new JwtSecurityTokenHandler().ReadJwtToken(user.AiiaRefreshToken),
                Providers = providers,
                ConsentId = user.AiiaConsentId,
                Email = user.Email,
                AllAccountsSelected = allAccountsSelected
            };

            return View(model);
        }
        catch (AiiaClientException ex)
        {
            if (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Probably the refresh code expired. Note here we should show a message that the token expired and
                // that we should reconnect the accounts, but to keep it simple we just show an empty page to allow the
                // user to reconnect.

                return View(new AccountsViewModel
                {
                    AccountsGroupedByProvider = null,
                    AiiaConnectUrl = _aiiaService.GetAuthUri(user?.Email).ToString(),
                    Providers = providers,
                    Email = user?.Email,
                    ConsentId = user?.AiiaConsentId
                });
            }

            throw;
        }

    }

    [HttpPost("{accountId}/transactions/query")]
    public async Task<IActionResult> FetchTransactions([FromRoute]string accountId, [FromBody] TransactionQueryRequestViewModel body)
    {
        var transactions = await _aiiaService.GetAccountTransactions(User, accountId, body);
        return Ok(new TransactionsViewModel(transactions.Transactions,
            transactions.PagingToken,
            body?.IncludeDeleted ?? false));
    }


    [HttpGet("{accountId}/transactions")]
    public async Task<IActionResult> Transactions(string accountId)
    {
        var transactions = await _aiiaService.GetAccountTransactions(User, accountId);
        return View(new TransactionsViewModel(transactions.Transactions, transactions.PagingToken, false));
    }
}