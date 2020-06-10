using System.Collections.Generic;
using ViiaSample.Models.Viia;

namespace ViiaSample.Models
{
    public class PaymentsViewModel
    {
        public IDictionary<Account, List<Payment>> PaymentsGroupedByAccountDisplayName { get; set; }
    }
}
