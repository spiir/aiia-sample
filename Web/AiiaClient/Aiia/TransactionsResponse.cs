using System.Collections.Generic;

namespace Aiia.Sample.Models.Aiia;

public class TransactionsResponse
{
    public string PagingToken { get; set; }
    public List<Transaction> Transactions { get; set; }
}