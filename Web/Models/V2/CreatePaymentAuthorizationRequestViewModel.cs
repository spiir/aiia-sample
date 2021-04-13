using System.Collections.Immutable;
using Aiia.Sample.Models.Aiia;

namespace Aiia.Sample.Models
{
    public class CreatePaymentAuthorizationRequestViewModel
    {
        public string Culture { get; set; }
        public string[] PaymentIds { get; set; }
        public string SourceAccountId { get; set; }
    }
}
