using System.Collections.Immutable;
using ViiaSample.Models.Viia;

namespace ViiaSample.Models
{
    public class PaymentsViewModel
    {
        public IImmutableList<Account> Accounts { get; set; }
    }
}