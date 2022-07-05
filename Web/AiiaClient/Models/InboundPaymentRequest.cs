namespace Aiia.Sample.AiiaClient.Models;

public class InboundPaymentRequest
{
    public PaymentAmountRequest Amount { get; set; }
    public string OrderId { get; set; }
    
    public string? Date { get; set; }
}