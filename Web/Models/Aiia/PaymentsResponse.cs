using System.Collections.Generic;

namespace Aiia.Sample.Models.Aiia
{
    public class PaymentsResponse
    {
        public string PagingToken { get; set; }
        public List<Payment> Payments { get; set; }
    }
}
