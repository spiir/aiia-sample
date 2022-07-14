using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Aiia.Sample.AiiaClient;
using Aiia.Sample.AiiaClient.Models;
using Aiia.Sample.Models;
using Aiia.Sample.Models.V2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Aiia.Sample.Controllers;

[Route("aiia/payments/outbound")]
[Authorize]
public class OutboundPaymentController : Controller
{
    private readonly IAiiaService _aiiaService;
    private readonly IWebHostEnvironment _environment;

    public OutboundPaymentController(IAiiaService aiiaService, IWebHostEnvironment environment)
    {
        _aiiaService = aiiaService;
        _environment = environment;
    }


    [HttpGet("create")]
    public async Task<IActionResult> CreatePayment()
    {
        if (_environment.IsProduction()) return NotFound();
        IImmutableList<Account> accounts = ImmutableList.Create<Account>();
        try
        {
            accounts = await _aiiaService.GetAccounts(User);
        }
        catch (AiiaClientException e)
        {
            // Ignored: If we fail to load accounts the drop down will be empty.
            // If this were a real commercial application, proper error handling would be needed to inform the user.
        }

        return View(new CreatePaymentViewModel
        {
            Accounts = accounts
        });
    }

    [HttpPost("create")]
    public async Task<ActionResult<CreatePaymentResultViewModelV2>> CreatePayment(
        [FromBody] CreatePaymentRequestViewModelV2 body)
    {
        if (_environment.IsProduction()) return NotFound();
        var result = new CreatePaymentResultViewModelV2();
        try
        {
            var createPaymentResult = await _aiiaService.CreateOutboundPaymentV2(User, body);
            result.PaymentId = createPaymentResult.PaymentId;
        }
        catch (AiiaClientException e)
        {
            result.ErrorDescription = e.Message;
        }

        return Ok(result);
    }

    [HttpGet("")]
    public async Task<IActionResult> Payments()
    {
        if (_environment.IsProduction()) return NotFound();
        var result = new PaymentsViewModel
        {
            PaymentsGroupedByAccountDisplayName = new Dictionary<Account, List<Payment>>()
        };
        
        var payments = await _aiiaService.GetPayments(User);
        var accounts = await _aiiaService.GetAccounts(User);
        foreach (var account in accounts)
        {
            var accountPayments = payments.Payments?.Where(payment =>
                payment.AccountId == account.Id && payment.Type == PaymentType.Outbound).ToList();
            result.PaymentsGroupedByAccountDisplayName.Add(account, accountPayments);
        }

        return View(result);
    }

    [HttpPost("payment-authorizations/create")]
    public async Task<ActionResult<CreatePaymentAuthorizationResultViewModel>> CreatePaymentAuthorization(
        [FromBody] CreatePaymentAuthorizationRequestViewModel body)
    {
        if (_environment.IsProduction()) return NotFound();
        var result = new CreatePaymentAuthorizationResultViewModel();
        try
        {
            var response = await _aiiaService.CreatePaymentAuthorization(User, body);
            result.AuthorizationId = response.AuthorizationId;
            result.AuthorizationUrl = response.AuthorizationUrl;
        }
        catch (AiiaClientException e)
        {
            result.ErrorDescription = e.Message;
        }

        return Ok(result);
    }

    [HttpGet("payment-authorizations/{accountId}/{authorizationId}")]
    public async Task<IActionResult> PaymentAuthorizations([FromRoute] string accountId,
        [FromRoute] string authorizationId)
    {
        if (_environment.IsProduction()) return NotFound();

        var authorization = await _aiiaService.GetPaymentAuthorization(User, accountId, authorizationId);
        return View("ViewAuthorization",  new ViewAuthorizationViewModel(authorization));
        
    }

    [HttpGet("payment-authorizations/callback")]
    public IActionResult PaymentAuthorizationCallback([FromQuery] string authorizationId)
    {
        if (_environment.IsProduction()) return NotFound();
        if (string.IsNullOrWhiteSpace(authorizationId))
            return View("PaymentAuthorizationCallback",
                new PaymentAuthorizationCallbackViewModel
                {
                    IsError = true
                });

        return View("PaymentAuthorizationCallback",
            new PaymentAuthorizationCallbackViewModel
            {
                Query = Request.QueryString.Value,
                PaymentAuthorizationId = authorizationId
            });
    }

    [HttpGet("{accountId}/{paymentId}")]
    public async Task<IActionResult> PaymentDetails([FromRoute] string accountId, [FromRoute] string paymentId)
    {
        var payment = await _aiiaService.GetOutboundPaymentV2(User, accountId, paymentId);
        return View("ViewOutboundPayment", new ViewPaymentV2ViewModel(payment));
    }
}
