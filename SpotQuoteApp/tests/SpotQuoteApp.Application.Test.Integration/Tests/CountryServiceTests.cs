using EventSourcingFramework.Core.Interfaces;
using SpotQuoteApp.Application.Interfaces;
using SpotQuoteApp.Core.AggregateRoots;

namespace SpotQuoteApp.Application.Test.Integration.Tests;

[Collection("Integration")]
public class CountryServiceTests : IAsyncLifetime
{
    private readonly ICountryService countryService;
    private readonly IRepository<Country> countryRepository;

    public CountryServiceTests(
        ICountryService countryService,
        IRepository<Country> countryRepository
    )
    {
        this.countryService = countryService;
        this.countryRepository = countryRepository;
    }

    public async Task InitializeAsync()
    {
        await countryRepository.CreateAsync(new Country("Denmark", "DK"));
        await countryRepository.CreateAsync(new Country("Sweden", "SE"));
    }

    public async Task DisposeAsync()
    {
        var all = await countryRepository.ReadAllAsync();
        foreach (var country in all)
            await countryRepository.DeleteAsync(country);
    }

    [Fact]
    public async Task GetAllCountriesAsync_Should_Return_All()
    {
        // Act
        var countries = await countryService.GetAllCountriesAsync();

        // Assert
        Assert.NotNull(countries);
        Assert.True(countries.Count >= 2);
        Assert.Contains(countries, c => c.Code == "DK");
        Assert.Contains(countries, c => c.Code == "SE");
    }

    [Fact]
    public async Task GetCountryByIdAsync_Should_Return_Correct_Country()
    {
        // Arrange
        var dk = await countryRepository.ReadByFilterAsync(c => c.Code == "DK");

        // Act
        var result = await countryService.GetCountryByIdAsync(dk!.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("DK", result.Code);
    }

    [Fact]
    public async Task GetCountryByCodeAsync_Should_Return_Correct_Country()
    {
        // Act
        var result = await countryService.GetCountryByCodeAsync("SE");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Sweden", result.Name);
    }

    [Fact]
    public async Task GetCountryByCodeAsync_Should_Return_Null_If_Not_Found()
    {
        // Act
        var result = await countryService.GetCountryByCodeAsync("XX");

        // Assert
        Assert.Null(result);
    }
}
