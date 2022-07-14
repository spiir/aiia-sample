using System;
using System.Collections.Generic;

namespace Aiia.Sample.AiiaClient.Models;

public class Transaction
{
    public string AccountId { get; set; }
    public Amount Balance { get; set; }
    public DateTimeOffset? Date { get; set; }
    public TransactionDetails Detail { get; set; }
    public string Id { get; set; }
    public string IsDeleted { get; set; }
    public string OriginalText { get; set; }
    public string State { get; set; }
    public string Text { get; set; }
    public Amount TransactionAmount { get; set; }
    public string Type { get; set; }
    public List<Category> Categories { get; set; }
}