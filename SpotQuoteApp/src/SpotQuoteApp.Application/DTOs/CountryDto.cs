namespace SpotQuoteApp.Application.DTOs;

public class CountryDto
{
    public Guid Id { get; set; }
    public int ConcurrencyVersion { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
}
