using System.Collections.Generic;

namespace ViiaSample.Models.Viia
{
    public class ViiaQueryPart
    {
        public List<string> IncludedQueryProperties { get; set; }
        public List<string> ExcludedQueryProperties { get; set; }
        public string Pattern { get; set; }
        public QueryPartOperator Operator { get; set; }
    }
}