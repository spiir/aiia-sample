using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using ViiaSample.Models;
using ViiaSample.Models.Viia;
using ViiaSample.Services;

namespace ViiaSample.Controllers
{
    [Route("viia/")]
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IViiaService _viiaService;

        public PaymentController(IViiaService viiaService, IWebHostEnvironment environment)
        {
            _viiaService = viiaService;
            _environment = environment;
        }

        [HttpGet("payments/create/outbound")]
        public async Task<IActionResult> CreateOutboundPayment()
        {
            if (_environment.IsProduction())
            {
                return NotFound();
            }
            IImmutableList<Account> accounts = ImmutableList.Create<Account>();
            try
            {
                accounts = await _viiaService.GetUserAccounts(User);
            }
            catch (ViiaClientException e)
            {
                // TODO
            }

            return View(new CreatePaymentViewModel
                        {
                            Accounts = accounts
                        });
        }

        [HttpGet("payments/create/inbound")]
        public async Task<IActionResult> CreateInboundPayment()
        {
            if (_environment.IsProduction())
            {
                return NotFound();
            }
            IImmutableList<Account> accounts = ImmutableList.Create<Account>();
            try
            {
                accounts = await _viiaService.GetUserAccounts(User);
            }
            catch (ViiaClientException e)
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
            if (_environment.IsProduction())
            {
                return NotFound();
            }
            var result = new CreatePaymentResultViewModel();
            try
            {
                var createPaymentResult = await _viiaService.CreateInboundPayment(User, body);
                result.PaymentId = createPaymentResult.PaymentId;
                result.AuthorizationUrl = createPaymentResult.AuthorizationUrl;
            }
            catch (ViiaClientException e)
            {
                result.ErrorDescription = e.Message;
            }

            return Ok(result);
        }

        [HttpPost("payments/create/outbound")]
        public async Task<ActionResult<CreatePaymentResultViewModel>> CreateOutboundPayment(
            [FromBody] CreatePaymentRequestViewModel body)
        {
            if (_environment.IsProduction())
            {
                return NotFound();
            }
            var result = new CreatePaymentResultViewModel();
            try
            {
                var createPaymentResult = await _viiaService.CreateOutboundPayment(User, body);
                result.PaymentId = createPaymentResult.PaymentId;
                result.AuthorizationUrl = createPaymentResult.AuthorizationUrl;
            }
            catch (ViiaClientException e)
            {
                result.ErrorDescription = e.Message;
            }

            return Ok(result);
        }

        [HttpGet("payments/callback")]
        public IActionResult PaymentCallback([FromQuery] string paymentId)
        {
            if (_environment.IsProduction())
            {
                return NotFound();
            }
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                return View("PaymentCallback",
                            new PaymentCallbackViewModel
                            {
                                IsError = true
                            });
            }

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
            if (_environment.IsProduction())
            {
                return NotFound();
            }
            try
            {
                var payment = await _viiaService.GetPayment(User, accountId, paymentId);
                return View(payment);
            }
            catch (ViiaClientException)
            {
                return View();
            }
        }

        [HttpGet("outboundpayments")]
        public async Task<IActionResult> OutboundPayments()
        {
            if (_environment.IsProduction())
            {
                return NotFound();
            }
            var result = new PaymentsViewModel
                         {
                             PaymentsGroupedByAccountDisplayName = new Dictionary<Account, List<Payment>>()
                         };
            try
            {
                var accounts = await _viiaService.GetUserAccounts(User);
                foreach (var account in accounts)
                {
                    var payments = await _viiaService.GetPayments(User);
                    result.PaymentsGroupedByAccountDisplayName.Add(
                                                                   account,
                                                                   payments.Payments);
                }
            }
            catch (ViiaClientException e)
            {
                // TODO
            }

            return View(result);
        }

        [HttpGet("inboundpayments")]
        public async Task<IActionResult> InboundPayments()
        {
            if (_environment.IsProduction())
            {
                return NotFound();
            }
            var result = new PaymentsViewModel
                         {
                             PaymentsGroupedByAccountDisplayName = new Dictionary<Account, List<Payment>>()
                         };
            try
            {
                var accounts = await _viiaService.GetUserAccounts(User);
                foreach (var account in accounts)
                {
                    var payments = await _viiaService.GetPayments(User);
                    result.PaymentsGroupedByAccountDisplayName.Add(
                                                                   account,
                                                                   payments.Payments);
                }
            }
            catch (ViiaClientException e)
            {
                // TODO
            }

            return View(result);
        }
    }
}
