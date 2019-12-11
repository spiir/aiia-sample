using System.Collections.Generic;

namespace ViiaSample.Models
{
    public class TransactionQueryRequestViewModel
    {
        public string PagingToken { get; set; }
        public bool IncludeDeleted { get; set; }
        public List<QueryPart> Filters { get; set; }
        public QueryFieldBetween AmountValueBetween { get; set; }
        public QueryFieldBetween BalanceValueBetween { get; set; }
    }

    public class QueryPart
    {
        public string Property { get; set; }
        public string Value { get; set; }
        public QueryPartOperator Operator { get; set; }
    }

    public enum QueryPartOperator
    {
        AND,
        OR
    }

    public class QueryFieldBetween
    {
        public double Min { get; set; }
        public double Max { get; set; }
    }
}