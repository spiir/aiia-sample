namespace Aiia.Sample.Models
{
    public class PaymentCallbackViewModel
    {
        public bool IsError { get; set; }
        public string PaymentId { get; set; }
        public string Query { get; set; }
    }
}
