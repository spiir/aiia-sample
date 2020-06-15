using NodaTime;

namespace ViiaSample.Models.Viia
{
    public class InboundPayment
    {
        public Amount Amount { get; set; }
        public Instant CreatedAt { get; set; }
        public string Id { get; set; }
        public PaymentIdentifiers Identifiers { get; set; }
        public PaymentReference SourceAccount { get; set; }
        public PaymentExecution Execution { get; set; }
        public PaymentStatus Status { get; set; }
    }
}