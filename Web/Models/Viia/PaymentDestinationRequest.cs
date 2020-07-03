namespace ViiaSample.Models.Viia
{
    public class PaymentDestinationRequest
    {
        public PaymentBBanRequest BBan { get; set; }
        public string IBan { get; set; }
        public string RecipientFullname { get; set; }
    }
}
