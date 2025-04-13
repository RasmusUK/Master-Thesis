using SpotQuoteBooking.EventSource.Application.DTOs;
using SpotQuoteBooking.EventSource.Core.ValueObjects.Enums;

namespace SpotQuoteBooking.EventSource.Application.Interfaces;

public interface ILocationService
{
    Task<Guid> CreateLocationIfNotExistsAsync(
        string code,
        string name,
        string countryCode,
        LocationType locationType
    );
}
