namespace ViiaSample.Models.Viia
{
    public class AccountNumber
    {
        public string BbanType { get; set; }
        public string Bban { get; set; }
        public string Iban { get; set; }
        public BbanParsed BbanParsed { get; set; }
    }
}