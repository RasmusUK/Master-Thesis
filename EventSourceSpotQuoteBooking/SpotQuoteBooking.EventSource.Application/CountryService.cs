using EventSource.Core.Interfaces;
using SpotQuoteBooking.EventSource.Application.DTOs;
using SpotQuoteBooking.EventSource.Application.Interfaces;
using SpotQuoteBooking.EventSource.Application.Mappers;
using SpotQuoteBooking.EventSource.Core.AggregateRoots;

namespace SpotQuoteBooking.EventSource.Application;

public class CountryService : ICountryService
{
    private readonly IRepository<Country> countryRepository;

    public CountryService(IRepository<Country> countryRepository)
    {
        this.countryRepository = countryRepository;
    }

    public async Task<IReadOnlyCollection<CountryDto>> GetAllCountriesAsync()
    {
        var countries = await countryRepository.ReadAllAsync();
        return countries.Select(CountryMapper.ToDto).ToList();
    }

    public async Task<CountryDto?> GetCountryByIdAsync(Guid countryId)
    {
        var country = await countryRepository.ReadByIdAsync(countryId);
        return country?.ToDto();
    }

    public async Task<CountryDto?> GetCountryByCodeAsync(string code)
    {
        var country = await countryRepository.ReadByFilterAsync(c => c.Code == code);
        return country?.ToDto();
    }
}
