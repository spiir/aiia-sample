namespace Aiia.Sample.AiiaClient.Models;

public class InboundPayment
{
    public Amount Amount { get; set; }
    public string CreatedAt { get; set; }
    public string Id { get; set; }
    public PaymentIdentifiers Identifiers { get; set; }
    public PaymentReference SourceAccount { get; set; }
    public PaymentExecution Execution { get; set; }
    public PaymentStatus Status { get; set; }
    public string PaymentMethod { get; set; }
    public PayerTokenModel PayerToken { get; set; }
}