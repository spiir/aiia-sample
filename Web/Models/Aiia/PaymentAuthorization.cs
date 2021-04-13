using NodaTime;

namespace Aiia.Sample.Models.Aiia
{
    public class PaymentAuthorization
    {
        public Instant CreatedAt { get; set; }
        public PaymentExecution Execution { get; set; }

        public string Id { get; set; }
        public string AccountId { get; set; }

        public PaymentStatus Status { get; set; }

        public PaymentType Type { get; set; }
    }
}
