namespace Aiia.Sample.AiiaClient.Models;

public class PaymentReference
{
    public BbanParsed Bban { get; set; }
    public IbanParsed Iban { get; set; }
    public string Name { get; set; }
    public InpaymentFormDestinationParsed InpaymentForm { get; set; }
    public PaymentAddressRequest Address { get; set; }
}