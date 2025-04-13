using SpotQuoteBooking.EventSource.Core.ValueObjects.Enums;

namespace SpotQuoteBooking.EventSource.Application.DTOs;

public class LocationDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public CountryDto Country { get; set; }
    public LocationType Type { get; set; }
}
