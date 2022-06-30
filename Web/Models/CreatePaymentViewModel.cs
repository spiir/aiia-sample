using System.Collections.Immutable;
using Aiia.Sample.Models.Aiia;

namespace Aiia.Sample.Models;

public class CreatePaymentViewModel
{
    public IImmutableList<Account> Accounts { get; set; }
}