namespace ViiaSample.Models.Viia
{
    public class CreateInboundPaymentRequest
    {
        public PaymentAmountRequest Amount { get; set; }
        public string Culture { get; set; }
        public string RedirectUrl { get; set; }
    }
}
