using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Application.Interfaces;

public interface ILocationService
{
    Task<Guid> CreateLocationIfNotExistsAsync(
        string code,
        string name,
        string countryCode,
        LocationType locationType
    );

    Task<LocationDto?> SearchLocationIdAsync(
        string code,
        string countryCode,
        LocationType locationType
    );
}
