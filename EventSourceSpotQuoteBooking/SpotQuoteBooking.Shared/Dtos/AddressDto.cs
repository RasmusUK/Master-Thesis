namespace SpotQuoteBooking.Shared.Dtos;

public class AddressDto
{
    public string CompanyName { get; set; }
    public string CountryCode { get; set; }
    public string CountryName { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Attention { get; set; }
    public string Port { get; set; }
    public string Airport { get; set; }

    public Address ToAddress()
    {
        return new Address(
            CompanyName,
            CountryCode,
            CountryName,
            City,
            PostalCode,
            AddressLine1,
            AddressLine2,
            Email,
            Phone,
            Attention,
            Port,
            Airport
        );
    }
}
