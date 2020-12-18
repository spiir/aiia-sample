using System.Collections.Generic;
using Aiia.Sample.Models.Aiia;

namespace Aiia.Sample.Models
{
    public class TransactionsViewModel
    {
        public bool IncludeDeleted { get; }
        public string PagingToken { get; }

        public List<Transaction> Transactions { get; }

        public TransactionsViewModel(List<Transaction> transactions, string pagingToken, bool includeDeleted)
        {
            Transactions = transactions;
            PagingToken = pagingToken;
            IncludeDeleted = includeDeleted;
        }
    }
}
