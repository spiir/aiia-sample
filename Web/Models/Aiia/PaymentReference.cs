namespace Aiia.Sample.Models.Aiia
{
    public class PaymentReference
    {
        public BbanParsed Bban { get; set; }
        public string Iban { get; set; }
        public string Name { get; set; }
    }
}