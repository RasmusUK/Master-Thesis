using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Application.DTOs;

public class LocationDto
{
    public Guid Id { get; set; }
    public int ConcurrencyVersion { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public CountryDto Country { get; set; }
    public LocationType Type { get; set; }
}
