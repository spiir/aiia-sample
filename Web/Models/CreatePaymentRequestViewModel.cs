namespace Aiia.Sample.Models;

public class CreatePaymentRequestViewModel
{
    public double Amount { get; set; }
    public string BbanAccountNumber { get; set; }
    public string BbanBankCode { get; set; }
    public string Culture { get; set; }
    public string Iban { get; set; }
    public string RecipientFullname { get; set; }
    public string message { get; set; }
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
    public bool IssuePayerToken { get; set; }
    public string PayerToken { get; set; }
    public string ProviderId { get; set; }
    public string OrderId { get; set; }
}