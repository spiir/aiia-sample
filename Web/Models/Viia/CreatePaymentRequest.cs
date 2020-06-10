namespace ViiaSample.Models.Viia
{
    public class CreatePaymentRequest
    {
        public string Culture { get; set; }
        public PaymentRequest Payment { get; set; }
        public string RedirectUrl { get; set; }
    }
}
