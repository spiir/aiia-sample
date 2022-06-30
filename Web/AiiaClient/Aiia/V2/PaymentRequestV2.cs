namespace Aiia.Sample.Models.Aiia;

public class PaymentRequestV2
{
    public PaymentAmountRequestV2 Amount { get; set; }
    public PaymentDestinationRequestV2 Destination { get; set; }
    public string Message { get; set; }
    public string TransactionText { get; set; }
    public string PaymentMethod { get; set; }
    public PaymentIdentifiersRequest Identifiers { get; set; }
}