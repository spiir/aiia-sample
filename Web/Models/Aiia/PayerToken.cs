using System;

namespace Aiia.Sample.Models.Aiia
{
    public class PayerToken
    {
        public string ProviderId { get; set; }
        public string RedactedAccountNumber { get; set; }
        public DateTime Expires { get; set; }
        public string Token { get; set; }
    }
}
