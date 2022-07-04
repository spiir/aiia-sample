namespace Aiia.Sample.AiiaClient.Models.V2;

public class CreatePaymentAuthorizationRequest
{
    public string Culture { get; set; }
    public string[] PaymentIds { get; set; }
    public string RedirectUrl { get; set; }
}