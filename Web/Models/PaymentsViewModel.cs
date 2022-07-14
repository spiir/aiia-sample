using System.Collections.Generic;
using Aiia.Sample.AiiaClient.Models;

namespace Aiia.Sample.Models;

public class PaymentsViewModel
{
    public IDictionary<Account, List<Payment>> PaymentsGroupedByAccountDisplayName { get; set; }
}