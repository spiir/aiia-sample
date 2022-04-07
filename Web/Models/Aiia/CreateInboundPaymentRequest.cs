namespace Aiia.Sample.Models.Aiia
{
    public class CreateInboundPaymentRequest
    {
        public InboundPaymentRequest Payment { get; set; }
        public string Culture { get; set; }
        public string RedirectUrl { get; set; }
        public bool IssuePayerToken { get; set; }
        public string PayerToken { get; set; }
        public string ProviderId { get; set; }
    }
}

