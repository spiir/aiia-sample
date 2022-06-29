namespace Aiia.Sample.Models
{
    public class CreatePaymentResultViewModel
    {
        public string ErrorDescription { get; set; }
        public string PaymentId { get; set; }
        public string AuthorizationUrl { get; set; }
    }
}