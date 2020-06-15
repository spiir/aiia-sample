using NodaTime;

namespace ViiaSample.Models.Viia
{
    public class OutboundPayment
    {
        public Amount Amount { get; set; }
        public Instant CreatedAt { get; set; }
        public PaymentReference Destination { get; set; }
        public string Id { get; set; }
        public PaymentIdentifiers Identifiers { get; set; }
        public PaymentStatus Status { get; set; }
        public string Type { get; set; }
    }
}