using NodaTime;

namespace ViiaSample.Models.Viia
{
    public class Payment
    {
        public Instant CreatedAt { get; set; }
        public PaymentExecution Execution { get; set; }

        public string Id { get; set; }

        public PaymentStatus Status { get; set; }
    }
}
