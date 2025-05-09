using EventSourcingFramework.Persistence;

namespace SpotQuoteApp.Core.AggregateRoots;

public class Address(
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
    ) : Entity
{
    public string CompanyName { get; set; } = companyName;
    public Guid CountryId { get; set; } = countryId;
    public string City { get; set; } = city;
    public string ZipCode { get; set; } = zipCode;
    public string AddressLine1 { get; set; } = addressLine1;
    public string AddressLine2 { get; set; } = addressLine2;
    public string Email { get; set; } = email;
    public string Phone { get; set; } = phone;
    public string Attention { get; set; } = attention;
    public string? Port { get; set; } = port;
    public string? Airport { get; set; } = airport;
}
