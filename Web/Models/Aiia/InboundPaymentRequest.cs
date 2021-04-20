namespace Aiia.Sample.Models.Aiia
{
    public class InboundPaymentRequest
    {
        public PaymentAmountRequest Amount { get; set; }
        public string PaymentMethod { get; set; }
    }
}