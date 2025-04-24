using EventSource.Persistence;

namespace SpotQuoteBooking.EventSource.Core.AggregateRoots;

public class Address : Entity
{
    public Address(
        string companyName,
        Guid countryId,
        string city,
        string zipCode,
        string addressLine1,
        string addressLine2,
        string email,
        string phone,
        string attention,
        string? port = null,
        string? airport = null
    )
    {
        CompanyName = companyName;
        CountryId = countryId;
        City = city;
        ZipCode = zipCode;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        Email = email;
        Phone = phone;
        Attention = attention;
        Port = port;
        Airport = airport;
    }

    public string CompanyName { get; set; }
    public Guid CountryId { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Attention { get; set; }
    public string? Port { get; set; }
    public string? Airport { get; set; }
}
