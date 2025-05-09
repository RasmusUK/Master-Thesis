using SpotQuoteApp.Application.DTOs;

namespace SpotQuoteApp.Application.Interfaces;

public interface ICountryService
{
    Task<IReadOnlyCollection<CountryDto>> GetAllCountriesAsync();
    Task<CountryDto?> GetCountryByIdAsync(Guid countryId);
    Task<CountryDto?> GetCountryByCodeAsync(string code);
}
