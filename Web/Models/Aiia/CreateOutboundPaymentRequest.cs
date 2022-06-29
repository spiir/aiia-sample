namespace Aiia.Sample.Models.Aiia
{
    public class CreateOutboundPaymentRequest
    {
        public string Culture { get; set; }
        public PaymentRequest Payment { get; set; }
        public string RedirectUrl { get; set; }
    }
}