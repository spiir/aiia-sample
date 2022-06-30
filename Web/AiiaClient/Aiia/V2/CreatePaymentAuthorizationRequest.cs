namespace Aiia.Sample.Models.Aiia;

public class CreatePaymentAuthorizationRequest
{
    public string Culture { get; set; }
    public string[] PaymentIds { get; set; }
    public string RedirectUrl { get; set; }
}