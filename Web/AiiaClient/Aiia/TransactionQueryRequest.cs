using System.Collections.Generic;
using Aiia.Sample.Models;
using Aiia.Sample.Models.Aiia;

namespace Aiia.Sample.Services;

public class TransactionQueryRequest
{
    public int PageSize { get; set; }
    public QueryFieldBetween BalanceValueBetween { get; set; }
    public QueryFieldBetween AmountValueBetween { get; set; }
    public string PagingToken { get; set; }
    public string Interval { get; set; }
}