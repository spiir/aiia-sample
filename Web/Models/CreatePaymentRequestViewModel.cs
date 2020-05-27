namespace ViiaSample.Models
{
    public class CreatePaymentRequestViewModel
    {
        public string SourceAccountId { get; set; }
        public string Iban { get; set; }
        public string BbanBankCode { get; set; }
        public string BbanAccountNumber { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public string message { get; set; }
        public string TransactionText { get; set; }
        public string ScheduledPaymentDate { get; set; }
        public string Culture { get; set; }
        public string PaymentType { get; set; }
    }

}