namespace Aiia.Sample.Models
{
    public class CreatePaymentRequestViewModelV2
    {
        public double Amount { get; set; }
        public string BbanAccountNumber { get; set; }
        public string BbanBankCode { get; set; }
        public string Iban { get; set; }
        public string RecipientFullname { get; set; }
        public string message { get; set; }
        public string SourceAccountId { get; set; }
        public string TransactionText { get; set; }
    }
}
