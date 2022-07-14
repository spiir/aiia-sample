namespace Aiia.Sample.AiiaClient.Models;

public class PaymentAddressRequest
{
    public string Street { get; set; }
    public string BuildingNumber { get; set; }
    public string PostalCode { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}