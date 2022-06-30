using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Aiia.Sample.Models;
using Aiia.Sample.Models.Aiia;
using Aiia.Sample.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Aiia.Sample.Controllers;

[Route("aiia/")]
[Authorize]
public class PaymentController : Controller
{
    private readonly IAiiaService _aiiaService;
    private readonly IWebHostEnvironment _environment;

    public PaymentController(IAiiaService aiiaService, IWebHostEnvironment environment)
    {
        _aiiaService = aiiaService;
        _environment = environment;
    }

    [HttpGet("payments/create/inbound")]
    public async Task<IActionResult> CreateInboundPayment()
    {
        if (_environment.IsProduction()) return NotFound();
        IImmutableList<Account> accounts = ImmutableList.Create<Account>();
        try
        {
            accounts = await _aiiaService.GetUserAccounts(User);
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

    [HttpPost("payments/create/inbound")]
    public async Task<ActionResult<CreatePaymentResultViewModel>> CreateInboundPayment(
        [FromBody] CreatePaymentRequestViewModel body)
    {
        if (_environment.IsProduction()) return NotFound();
        var result = new CreatePaymentResultViewModel();
        try
        {
            var createPaymentResult = await _aiiaService.CreateInboundPayment(User, body);
            result.PaymentId = createPaymentResult.PaymentId;
            result.AuthorizationUrl = createPaymentResult.AuthorizationUrl;
        }
        catch (AiiaClientException e)
        {
            result.ErrorDescription = e.Message;
        }

        return Ok(result);
    }

    [HttpGet("payments/create/outbound")]
    public async Task<IActionResult> CreateOutboundPayment()
    {
        if (_environment.IsProduction()) return NotFound();
        IImmutableList<Account> accounts = ImmutableList.Create<Account>();
        try
        {
            accounts = await _aiiaService.GetUserAccounts(User);
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

    [HttpPost("payments/create/outbound")]
    public async Task<ActionResult<CreatePaymentResultViewModel>> CreateOutboundPayment(
        [FromBody] CreatePaymentRequestViewModel body)
    {
        if (_environment.IsProduction()) return NotFound();
        var result = new CreatePaymentResultViewModel();
        try
        {
            var createPaymentResult = await _aiiaService.CreateOutboundPayment(User, body);
            result.PaymentId = createPaymentResult.PaymentId;
            result.AuthorizationUrl = createPaymentResult.AuthorizationUrl;
        }
        catch (AiiaClientException e)
        {
            result.ErrorDescription = e.Message;
        }

        return Ok(result);
    }

    [HttpGet("payments/inbound")]
    public async Task<IActionResult> InboundPayments()
    {
        if (_environment.IsProduction()) return NotFound();
        var result = new PaymentsViewModel
        {
            PaymentsGroupedByAccountDisplayName = new Dictionary<Account, List<Payment>>()
        };
        try
        {
            var payments = await _aiiaService.GetPayments(User);
            var accounts = await _aiiaService.GetUserAccounts(User);
            foreach (var account in accounts)
            {
                var accountPayments = payments.Payments?.Where(payment =>
                    payment.AccountId == account.Id && payment.Type == PaymentType.Inbound).ToList();
                result.PaymentsGroupedByAccountDisplayName.Add(account, accountPayments);
            }
        }
        catch (AiiaClientException e)
        {
            // TODO
        }

        return View(result);
    }

    [HttpGet("payments/outbound")]
    public async Task<IActionResult> OutboundPayments()
    {
        if (_environment.IsProduction()) return NotFound();
        var result = new PaymentsViewModel
        {
            PaymentsGroupedByAccountDisplayName = new Dictionary<Account, List<Payment>>()
        };
        try
        {
            var payments = await _aiiaService.GetPayments(User);
            var accounts = await _aiiaService.GetUserAccounts(User);
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

    [HttpGet("payments/callback")]
    public IActionResult PaymentCallback([FromQuery] string paymentId)
    {
        if (_environment.IsProduction()) return NotFound();
        if (string.IsNullOrWhiteSpace(paymentId))
            return View("PaymentCallback",
                new PaymentCallbackViewModel
                {
                    IsError = true
                });

        return View("PaymentCallback",
            new PaymentCallbackViewModel
            {
                Query = Request.QueryString.Value,
                PaymentId = paymentId
            });
    }

    [HttpGet("accounts/{accountId}/payments/{paymentId}")]
    public async Task<IActionResult> PaymentDetails([FromRoute] string accountId, [FromRoute] string paymentId)
    {
        if (_environment.IsProduction()) return NotFound();
        try
        {
            var payment = await _aiiaService.GetOutboundPayment(User, accountId, paymentId);
            return View("PaymentDetails", payment);
        }
        catch (AiiaClientException)
        {
            try
            {
                var payment = await _aiiaService.GetInboundPayment(User, accountId, paymentId);
                return View("PaymentDetails", payment);
            }
            catch (AiiaClientException)
            {
                return View("PaymentDetails");
            }
        }
    }
}