using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Aiia.Sample.AiiaClient;
using Aiia.Sample.AiiaClient.Models;
using Aiia.Sample.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aiia.Sample.Controllers;

[Route("aiia/payments/inbound")]
[Authorize]
public class InboundPaymentController : Controller
{
    private readonly IAiiaService _aiiaService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<InboundPaymentController> _logger;

    public InboundPaymentController(IAiiaService aiiaService, IWebHostEnvironment environment, ILogger<InboundPaymentController> logger)
    {
        _aiiaService = aiiaService;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet("create")]
    public async Task<IActionResult> CreateInboundPayment()
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
            _logger.LogWarning(e, "Something went wrong calling aiia-api");

            result.ErrorDescription = "Something went wrong calling aiia-api";
        }

        return Ok(result);
    }

    
    [HttpGet("")]
    public async Task<IActionResult> InboundPayments()
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
                payment.AccountId == account.Id && payment.Type == PaymentType.Inbound).ToList();
            result.PaymentsGroupedByAccountDisplayName.Add(account, accountPayments);
        }

        return View(result);
    }

    [HttpGet("callback")]
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

    [HttpGet("{accountId}/{paymentId}")]
    public async Task<IActionResult> PaymentDetails([FromRoute] string accountId, [FromRoute] string paymentId)
    {
        if (_environment.IsProduction()) return NotFound();
        
        // try fetching reconciliation information. This will fail for payments that are not inbound to an Aiia-managed account.
        PaymentReconciliationV1Response reconciliation = null;
        try
        {
            reconciliation = await _aiiaService.GetPaymentReconciliationV1(User, accountId, paymentId);
        }
        catch (Exception e)
        {
            // ignore if we fail to fetch the reconciliation information.
        }

        // fetch the payment
        var payment = await _aiiaService.GetInboundPayment(User, accountId, paymentId);
        var viewModel = new ViewPaymentV1ViewModel(payment, PaymentType.Inbound, reconciliation);
        viewModel.PayerToken = payment.PayerToken;
        return View("ViewInboundPayment", viewModel);
    }
}