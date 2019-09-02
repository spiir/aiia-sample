using System.Collections.Generic;

namespace ViiaSample.Models.Viia
{
    public class TransactionsResponse
    {
        public List<Transaction> Transactions { get; set; }
        public string PagingToken { get; set; }
    }
}