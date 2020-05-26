namespace ViiaSample.Models.Viia
{
    public class PaymentRequest
    {
        public PaymentAmountRequest Amount { get; set; }
        public PaymentDestinationRequest Destination { get; set; }
        public string Message { get; set; }
        public string TransactionText { get; set; }
    }
}