namespace Aiia.Sample.AiiaClient.Models.V2;

public class CreateOutboundPaymentRequestV2
{
    public string Culture { get; set; }
    public PaymentRequestV2 Payment { get; set; }
    public string RedirectUrl { get; set; }
}