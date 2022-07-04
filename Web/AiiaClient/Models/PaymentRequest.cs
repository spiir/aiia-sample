namespace Aiia.Sample.AiiaClient.Models;

public class PaymentRequest
{
    public PaymentAmountRequest Amount { get; set; }
    public PaymentDestinationRequest Destination { get; set; }
    public string Message { get; set; }
    public string TransactionText { get; set; }
    public string PaymentMethod { get; set; }
    public PaymentIdentifiersRequest Identifiers { get; set; }
}