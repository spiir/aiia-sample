using System.Collections.Generic;

namespace Aiia.Sample.Models.Aiia;

public class AiiaQueryPart
{
    public List<string> ExcludedQueryProperties { get; set; }
    public List<string> IncludedQueryProperties { get; set; }
    public QueryPartOperator Operator { get; set; }
    public string Pattern { get; set; }
}