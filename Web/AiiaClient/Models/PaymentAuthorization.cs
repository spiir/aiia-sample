namespace Aiia.Sample.AiiaClient.Models;

public class PaymentAuthorization
{
    public string CreatedAt { get; set; } // TODO: This was Instant
    public PaymentExecution Execution { get; set; }

    public string Id { get; set; }
    public string AccountId { get; set; }

    public PaymentStatus Status { get; set; }

    public PaymentType Type { get; set; }
}