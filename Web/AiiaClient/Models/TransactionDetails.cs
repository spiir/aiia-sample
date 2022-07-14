namespace Aiia.Sample.AiiaClient.Models;

public class TransactionDetails
{
    public CurrencyConversionViewModel CurrencyConversion { get; set; }
    public TransactionPartyViewModel Destination { get; set; }
    // This string is a date. Here we use string to keep it simple, but it should be an appropriate type like NodaTime's LocalDate
    public string? ExecutionDate { get; set; }
    public TransactionIdentifiersViewModel Identifiers { get; set; }
    public string Message { get; set; }
    public RewardViewModel Reward { get; set; }
    public TransactionPartyViewModel Source { get; set; }
    // This string is a date. Here we use string to keep it simple, but it should be an appropriate type like NodaTime's LocalDate
    public string? ValueDate { get; set; }
}

public class CurrencyConversionViewModel
{
    public decimal? ExchangeRate { get; set; }
    public Amount OriginalAmount { get; set; }
}

public class TransactionPartyViewModel
{
    public AccountNumber Account { get; set; }
    public string Address { get; set; }
    public string MerchantCategoryCode { get; set; }
    public string MerchantCategoryName { get; set; }
    public string Name { get; set; }
}

public class TransactionIdentifiersViewModel
{
    public string CreditorReference { get; set; }
    public string Document { get; set; }
    public string EndToEndId { get; set; }
    public string Reference { get; set; }
    public string SequenceNumber { get; set; }
    public string Terminal { get; set; }
}

public class RewardViewModel
{
    public Amount Amount { get; set; }
    public decimal? Points { get; set; }
    public string Type { get; set; }
}