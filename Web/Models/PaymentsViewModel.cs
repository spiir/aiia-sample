using System.Collections.Generic;
using System.Linq;
using ViiaSample.Models.Viia;

namespace ViiaSample.Models
{
    public class PaymentsViewModel
    {
        public IDictionary<Account, List<Payment>> PaymentsGroupedByAccountDisplayName { get; set; }
    }
}