using System.Collections.Generic;

namespace ViiaSample.Models.Viia
{
    public class Category
    {
        public string Id { get; set; }
        public IDictionary<string, string> Names { get; set; }
        public string ParentId { get; set; }
        public string SetId { get; set; }
        public double Score { get; set; }
    }
}