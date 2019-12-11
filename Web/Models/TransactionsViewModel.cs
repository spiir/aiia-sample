using System.Collections.Generic;
using ViiaSample.Models.Viia;

namespace ViiaSample.Models
{
    public class TransactionsViewModel
    {
        public TransactionsViewModel(List<Transaction> transactions, string pagingToken, bool includeDeleted)
        {
            Transactions = transactions;
            PagingToken = pagingToken;
            IncludeDeleted = includeDeleted;
        }

        public List<Transaction> Transactions { get; }
        public string PagingToken { get; }
        public bool IncludeDeleted { get; }
    }
}