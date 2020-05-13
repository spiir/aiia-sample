using System.Collections.Generic;

namespace ViiaSample.Models.Viia
{
    public class ViiaQueryPart
    {
        public List<string> ExcludedQueryProperties { get; set; }
        public List<string> IncludedQueryProperties { get; set; }
        public QueryPartOperator Operator { get; set; }
        public string Pattern { get; set; }
    }
}
