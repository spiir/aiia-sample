namespace Aiia.Sample.Models.V2;

public class CreatePaymentAuthorizationRequestViewModel
{
    public string Culture { get; set; }
    public string[] PaymentIds { get; set; }
    public string SourceAccountId { get; set; }
}