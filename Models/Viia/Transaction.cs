using System;

namespace ViiaSample.Models.Viia
{
    public class Transaction
    {
        public string Id { get; set; }
        public DateTimeOffset? Date { get; set; }
        public Amount Balance { get; set; }
        public Amount TransactionAmount { get; set; }
        public string Text { get; set; }
        public string OriginalText { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
    }
}