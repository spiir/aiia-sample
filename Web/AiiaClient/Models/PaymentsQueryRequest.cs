namespace Aiia.Sample.AiiaClient.Models;

public class PaymentsQueryRequest
{
    public string Interval { get; set; } // TODO: This was Interval?
    public int PageSize { get; set; }
    public string PagingToken { get; set; }
}