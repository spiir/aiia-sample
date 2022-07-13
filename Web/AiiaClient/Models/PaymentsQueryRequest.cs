namespace Aiia.Sample.AiiaClient.Models;

public class PaymentsQueryRequest
{
    // This string is an interval of time. Here we use string to keep it simple, but it should be an appropriate type like NodaTime's Interval.
    public string Interval { get; set; } 
    public int PageSize { get; set; }
    public string PagingToken { get; set; }
}