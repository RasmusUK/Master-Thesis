using SpotQuoteBooking.Shared.Data;

namespace SpotQuoteBooking.Shared;

public class Address
{
    public string CompanyName { get; set; }
    public Country Country { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Attention { get; set; }
    public string? Port { get; set; }
    public string? Airport { get; set; }

    public Address() { }

    public Address(
        string companyName,
        Country country,
        string city,
        string zipCode,
        string addressLine1,
        string addressLine2,
        string email,
        string phone,
        string attention,
        string? port,
        string? airport
    )
    {
        CompanyName = companyName;
        Country = country;
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
}
