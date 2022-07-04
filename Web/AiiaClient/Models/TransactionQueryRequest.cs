using Aiia.Sample.Models;

namespace Aiia.Sample.AiiaClient.Models;

public class TransactionQueryRequest
{
    public int PageSize { get; set; }
    public QueryFieldBetween BalanceValueBetween { get; set; }
    public QueryFieldBetween AmountValueBetween { get; set; }
    public string PagingToken { get; set; }
    public string Interval { get; set; }
}