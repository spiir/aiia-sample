namespace Aiia.Sample.Models.Aiia
{
    public class PaymentDestinationRequestV2
    {
        public PaymentBBanRequest BBan { get; set; }
        public PaymentIbanRequestV2 IBan { get; set; }
        public string Name { get; set; }
        public PaymentInpaymentFormRequest InpaymentForm { get; set; }
        public PaymentAddressRequest Address { get; set; }
    }
}
