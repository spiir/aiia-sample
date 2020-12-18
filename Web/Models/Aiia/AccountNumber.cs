namespace Aiia.Sample.Models.Aiia
{
    public class AccountNumber
    {
        public string Bban { get; set; }
        public BbanParsed BbanParsed { get; set; }
        public string BbanType { get; set; }
        public string Iban { get; set; }
    }
}
