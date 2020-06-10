using System.Collections.Immutable;
using NodaTime;

namespace ViiaSample.Models.Viia
{
    public class PaymentsQueryRequest
    {
        public Interval? Interval { get; set; }
        public int PageSize { get; set; }
        public string PagingToken { get; set; }
        public IImmutableList<QueryPart> Patterns { get; set; }
    }
}
