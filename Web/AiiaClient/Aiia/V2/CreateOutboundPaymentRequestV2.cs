namespace Aiia.Sample.Models.Aiia;

public class CreateOutboundPaymentRequestV2
{
    public string Culture { get; set; }
    public PaymentRequestV2 Payment { get; set; }
    public string RedirectUrl { get; set; }
}