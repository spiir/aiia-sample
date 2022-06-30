using System.Collections.Generic;

namespace Aiia.Sample.Models;

public class TransactionQueryRequestViewModel
{
    public QueryFieldBetween AmountValueBetween { get; set; }

    public QueryFieldBetween BalanceValueBetween { get; set; }

    // TODO: remove filters
    public List<QueryPart> Filters { get; set; }
    public bool IncludeDeleted { get; set; }
    public string PagingToken { get; set; }
}

public class QueryPart
{
    public QueryPartOperator Operator { get; set; }
    public string Property { get; set; }
    public string Value { get; set; }
}

public enum QueryPartOperator
{
    AND,
    OR
}

public class QueryFieldBetween
{
    public double Max { get; set; }
    public double Min { get; set; }
}