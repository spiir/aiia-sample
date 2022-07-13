namespace Aiia.Sample.AiiaClient.Models;

public class PaymentAuthorization
{
    // This string is a date-time. Here we use string to keep it simple, but it should be an appropriate type like NodaTime's Instant
    public string CreatedAt { get; set; }
    public PaymentExecution Execution { get; set; }

    public string Id { get; set; }
    public string AccountId { get; set; }

    public PaymentStatus Status { get; set; }

    public PaymentType Type { get; set; }
}