namespace SpotQuoteBooking.EventSource.Core;

public record Address(
    string CompanyName,
    string CountryCode,
    string CountryName,
    string City,
    string PostalCode,
    string AddressLine1,
    string AddressLine2,
    string Email,
    string Phone,
    string Attention,
    string Port,
    string Airport
);
