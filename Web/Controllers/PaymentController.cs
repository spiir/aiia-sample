using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViiaSample.Models;
using ViiaSample.Models.Viia;
using ViiaSample.Services;

namespace ViiaSample.Controllers
{
    [Route("viia/")]
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IViiaService _viiaService;

        public PaymentController(IViiaService viiaService)
        {
            _viiaService = viiaService;
        }

        [HttpGet("payments")]
        public async Task<IActionResult> Payments()
        {
            var result = new PaymentsViewModel
            {
                PaymentsGroupedByAccountDisplayName = new Dictionary<Account, List<Payment>>()
            };
            try
            {
                var accounts = await _viiaService.GetUserAccounts(User);
                foreach (var account in accounts)
                {
                    var payments = await _viiaService.GetPayments(User, account.Id);
                    result.PaymentsGroupedByAccountDisplayName.Add(
                        account, payments.Payments);
                }
            }
            catch (ViiaClientException e)
            {
                // TODO
            }

            return View(result);
        }

        [HttpGet("accounts/{accountId}/payments/{paymentId}")]
        public async Task<IActionResult> PaymentDetails([FromRoute] string accountId, [FromRoute] string paymentId)
        {
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

        [HttpGet("payments/create")]
        public async Task<IActionResult> CreatePayment()
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

            return View(new CreatePaymentViewModel
            {
                Accounts = accounts
            });
        }

        [HttpPost("payments/create")]
        public async Task<ActionResult<CreatePaymentResultViewModel>> CreatePayment(
            [FromBody] CreatePaymentRequestViewModel body)
        {
            var result = new CreatePaymentResultViewModel();
            try
            {
                var createPaymentResult = await _viiaService.CreatePayment(User, body);
                result.PaymentId = createPaymentResult.PaymentId;
                result.PaymentUrl = createPaymentResult.PaymentUrl;
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
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                return View("PaymentCallback", new PaymentCallbackViewModel
                {
                    IsError = true
                });
            }

            return View("PaymentCallback", new PaymentCallbackViewModel
            {
                Query = Request.QueryString.Value,
                PaymentId = paymentId
            });
        }
    }
}