using System.Collections.Immutable;

namespace Aiia.Sample.Models.Aiia
{
    public class PaymentsQueryRequest
    {
        public string Interval { get; set; } // TODO: This was Interval?
        public int PageSize { get; set; }
        public string PagingToken { get; set; }
        public IImmutableList<QueryPart> Patterns { get; set; }
    }
}