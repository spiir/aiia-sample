namespace Aiia.Sample.Models.V2;

public class CreatePaymentAuthorizationResultViewModel
{
    public string ErrorDescription { get; set; }
    public string AuthorizationId { get; set; }
    public string AuthorizationUrl { get; set; }
}