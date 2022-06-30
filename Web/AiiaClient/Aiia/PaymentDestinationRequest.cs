namespace Aiia.Sample.Models.Aiia;

public class PaymentDestinationRequest
{
    public PaymentBBanRequest BBan { get; set; }
    public string IBan { get; set; }
    public string RecipientFullname { get; set; }
    public PaymentInpaymentFormRequest InpaymentForm { get; set; }
    public PaymentAddressRequest Address { get; set; }
}