using System;

namespace Aiia.Sample.Models.Aiia
{
    public class PayerTokenModel
    {
        public string ProviderId { get; set; }
        public string RedactedAccountNumber { get; set; }
        public DateTime Expires { get; set; }
        public string PayerToken { get; set; }
    }
}