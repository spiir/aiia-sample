using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Aiia.Sample.AiiaClient;
using Aiia.Sample.AiiaClient.Models;
using Aiia.Sample.Models;
using Aiia.Sample.Models.V2;
using Aiia.Sample.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Aiia.Sample.Controllers;

[Route("v2/aiia/")]
[Authorize]
public class PaymentV2Controller : Controller
{
    private readonly IAiiaService _aiiaService;
    private readonly IWebHostEnvironment _environment;

    public PaymentV2Controller(IAiiaService aiiaService, IWebHostEnvironment environment)
    {
        _aiiaService = aiiaService;
        _environment = environment;
    }


    [HttpGet("payments/create")]
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
            // TODO
        }

        return View(new CreatePaymentViewModel
        {
            Accounts = accounts
        });
    }

    [HttpPost("payments/create")]
    public async Task<ActionResult<CreatePaymentResultViewModelV2>> CreatePayment(
        [FromBody] CreatePaymentRequestViewModelV2 body)
    {
        if (_environment.IsProduction()) return NotFound();
        var result = new CreatePaymentResultViewModelV2();
        try
        {
            var createPaymentResult = await _aiiaService.CreatePaymentV2(User, body);
            result.PaymentId = createPaymentResult.PaymentId;
        }
        catch (AiiaClientException e)
        {
            result.ErrorDescription = e.Message;
        }

        return Ok(result);
    }

    [HttpGet("payments")]
    public async Task<IActionResult> Payments()
    {
        if (_environment.IsProduction()) return NotFound();
        var result = new PaymentsViewModel
        {
            PaymentsGroupedByAccountDisplayName = new Dictionary<Account, List<Payment>>()
        };
        try
        {
            var payments = await _aiiaService.GetPayments(User);
            var accounts = await _aiiaService.GetAccounts(User);
            foreach (var account in accounts)
            {
                var accountPayments = payments.Payments?.Where(payment =>
                    payment.AccountId == account.Id && payment.Type == PaymentType.Outbound).ToList();
                result.PaymentsGroupedByAccountDisplayName.Add(account, accountPayments);
            }
        }
        catch (AiiaClientException e)
        {
            // TODO
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
        [FromRoute] string paymentAuthorizationId)
    {
        if (_environment.IsProduction()) return NotFound();

        return NotFound();
        
        /*
        try
        {
            var authorization = await _aiiaService.GetPaymentAuthorization(User, accountId, paymentAuthorizationId);
            return View("ObjectDetailsView",  new ObjectDetailsViewModel("Payment Authorization", authorization, authorization.Id));
        }
        catch (AiiaClientException)
        {
            return View("ObjectDetailsView");
        }
        */
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

    [HttpGet("accounts/{accountId}/payments/{paymentId}")]
    public async Task<IActionResult> PaymentDetails([FromRoute] string accountId, [FromRoute] string paymentId)
    {
        var payment = await _aiiaService.GetOutboundPaymentV2(User, accountId, paymentId);
        return View("ViewPaymentV2", new ViewPaymentV2ViewModel(payment));
    }
}