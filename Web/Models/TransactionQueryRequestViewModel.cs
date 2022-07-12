namespace Aiia.Sample.Models;

public class TransactionQueryRequestViewModel
{
    public QueryFieldBetween AmountValueBetween { get; set; }

    public QueryFieldBetween BalanceValueBetween { get; set; }

    public bool IncludeDeleted { get; set; }
    public string PagingToken { get; set; }
}


public class QueryFieldBetween
{
    public double Max { get; set; }
    public double Min { get; set; }
}