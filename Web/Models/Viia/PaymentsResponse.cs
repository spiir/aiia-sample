using System.Collections.Generic;

namespace ViiaSample.Models.Viia
{
    public class PaymentsResponse
    {
        public string PagingToken { get; set; }
        public List<Payment> Payments { get; set; }
    }
}