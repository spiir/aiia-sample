using System.Collections.Generic;
using Aiia.Sample.AiiaClient.Models;

namespace Aiia.Sample.Models;

public class TransactionsViewModel
{
    public TransactionsViewModel(List<Transaction> transactions, string pagingToken, bool includeDeleted)
    {
        Transactions = transactions;
        PagingToken = pagingToken;
        IncludeDeleted = includeDeleted;
    }

    public bool IncludeDeleted { get; }
    public string PagingToken { get; }

    public List<Transaction> Transactions { get; }
}