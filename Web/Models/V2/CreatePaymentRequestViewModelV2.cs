namespace Aiia.Sample.Models.V2;

public class CreatePaymentRequestViewModelV2
{
    public double Amount { get; set; }
    public string Currency { get; set; }
    public string BbanAccountNumber { get; set; }
    public string BbanBankCode { get; set; }
    public string Iban { get; set; }
    public string RecipientFullname { get; set; }
    public string Message { get; set; }
    public string SourceAccountId { get; set; }
    public string TransactionText { get; set; }
    public string PaymentMethod { get; set; }
    public string InpaymentFormType { get; set; }
    public string InpaymentFormCreditorNumber { get; set; }
    public string Ocr { get; set; }
    public string AddressStreet { get; set; }
    public string AddressBuildingNumber { get; set; }
    public string AddressPostalCode { get; set; }
    public string AddressCity { get; set; }
    public string AddressCountry { get; set; }
}