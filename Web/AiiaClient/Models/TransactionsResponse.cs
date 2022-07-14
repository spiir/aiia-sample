using System.Collections.Generic;

namespace Aiia.Sample.AiiaClient.Models;

public class TransactionsResponse
{
    public string PagingToken { get; set; }
    public List<Transaction> Transactions { get; set; }
}