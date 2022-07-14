using System.Collections.Immutable;
using Aiia.Sample.AiiaClient.Models;

namespace Aiia.Sample.Models;

public class CreatePaymentViewModel
{
    public IImmutableList<Account> Accounts { get; set; }
}