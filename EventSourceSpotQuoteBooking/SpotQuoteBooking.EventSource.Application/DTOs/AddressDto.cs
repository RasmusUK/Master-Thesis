namespace SpotQuoteBooking.EventSource.Application.DTOs;

public class AddressDto
{
    public Guid Id { get; set; }
    public int ConcurrencyVersion { get; set; }
    public string CompanyName { get; set; }
    public CountryDto Country { get; set; }
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
