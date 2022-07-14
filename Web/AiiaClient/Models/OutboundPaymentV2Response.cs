namespace Aiia.Sample.AiiaClient.Models;

public class OutboundPaymentV2Response
{
        public PaymentAmountViewModel Amount { get; set; }

        // This string is a date. Here we use string to keep it simple, but it should be an appropriate type like NodaTime's LocalDate
        public string? CreatedAt { get; set; }

        public PaymentReferenceViewModel Destination { get; set; }

        public PaymentExecutionOptionsViewModel ExecutionOptions { get; set; }

        public string Id { get; set; }

        public PaymentIdentifiersViewModel Identifiers { get; set; }

        public string Message { get; set; }

        public string PaymentMethod { get; set; }

        public PaymentStatus Status { get; set; }
}

public class PaymentAmountViewModel
{
        public string Currency { get; set; }
        public decimal Value { get; set; }
}

public class PaymentReferenceViewModel
{
        public BbanParsed Bban { get; set; }

        public IbanParsed Iban { get; set; }

        public string Name { get; set; }

        public InpaymentFormViewModel InpaymentForm { get; set; }

        public PaymentAddressViewModel Address { get; set; }
}
public class InpaymentFormViewModel
{
        public string Type { get; set; }
        public string CreditorNumber { get; set; }
}

public class PaymentAddressViewModel
{
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
}

public class PaymentIdentifiersViewModel
{
        public string CreditorReference { get; set; }

        public string EndToEndId { get; set; }

        public string FinnishArchiveId { get; set; }

        public string FinnishReference { get; set; }

        public string NorwegianKid { get; set; }

        public string Ocr { get; set; }
}

public class PaymentExecutionOptionsViewModel
{
        public string Type { get; set; }
        
        // This string is a date. Here we use string to keep it simple, but it should be an appropriate type like NodaTime's LocalDate
        public string? Date { get; set; }
}