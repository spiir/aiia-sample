namespace Aiia.Sample.Models.V2;

public class PaymentAuthorizationCallbackViewModel
{
    public bool IsError { get; set; }
    public string PaymentAuthorizationId { get; set; }
    public string Query { get; set; }
}