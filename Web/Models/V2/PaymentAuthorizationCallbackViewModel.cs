namespace Aiia.Sample.Models
{
    public class PaymentAuthorizationCallbackViewModel
    {
        public bool IsError { get; set; }
        public string PaymentAuthorizationId { get; set; }
        public string Query { get; set; }
    }
}
