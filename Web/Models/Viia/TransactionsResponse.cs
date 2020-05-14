using System.Collections.Generic;

namespace ViiaSample.Models.Viia
{
    public class TransactionsResponse
    {
        public string PagingToken { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
