using EventSourcingFramework.Core.Interfaces;
using SpotQuoteApp.Application.Interfaces;
using SpotQuoteApp.Core.DomainObjects;
using SpotQuoteApp.Core.Exceptions;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Application.Test.Integration.Tests;

[Collection("Integration")]
public class LocationServiceTests : IAsyncLifetime
{
    private readonly ILocationService locationService;
    private readonly IRepository<Location> locationRepository;
    private readonly IRepository<Country> countryRepository;

    public LocationServiceTests(
        ILocationService locationService,
        IRepository<Location> locationRepository,
        IRepository<Country> countryRepository
    )
    {
        this.locationService = locationService;
        this.locationRepository = locationRepository;
        this.countryRepository = countryRepository;
    }

    public async Task InitializeAsync()
    {
        await countryRepository.CreateAsync(new Country("Denmark", "DK"));
    }

    public async Task DisposeAsync()
    {
        var locations = await locationRepository.ReadAllAsync();
        foreach (var loc in locations)
            await locationRepository.DeleteAsync(loc);

        var countries = await countryRepository.ReadAllAsync();
        foreach (var c in countries)
            await countryRepository.DeleteAsync(c);
    }

    [Fact]
    public async Task CreateLocationIfNotExistsAsync_Should_Create_New_Location()
    {
        // Arrange
        var code = "1000";
        var name = "Copenhagen";
        var type = LocationType.ZipCode;

        // Act
        var id = await locationService.CreateLocationIfNotExistsAsync(code, name, "DK", type);

        // Assert
        Assert.NotEqual(Guid.Empty, id);
        var stored = await locationRepository.ReadByIdAsync(id);
        Assert.Equal(code, stored?.Code);
        Assert.Equal(name, stored?.Name);
        Assert.Equal(type, stored?.Type);
    }

    [Fact]
    public async Task CreateLocationIfNotExistsAsync_Should_Return_Existing_If_Found()
    {
        // Arrange
        var code = "2000";
        var name = "Frederiksberg";
        var country = await countryRepository.ReadByFilterAsync(c => c.Code == "DK");

        var location = new Location(code, name, country!.Id, LocationType.ZipCode);
        await locationRepository.CreateAsync(location);

        // Act
        var resultId = await locationService.CreateLocationIfNotExistsAsync(
            code,
            name,
            "DK",
            LocationType.ZipCode
        );

        // Assert
        Assert.Equal(location.Id, resultId);
    }

    [Fact]
    public async Task CreateLocationIfNotExistsAsync_Throws_If_Country_Not_Found()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () =>
                locationService.CreateLocationIfNotExistsAsync(
                    "3000",
                    "Helsingør",
                    "XX",
                    LocationType.ZipCode
                )
        );

        Assert.Contains("Country with code", ex.Message);
    }

    [Fact]
    public async Task SearchLocationIdAsync_Returns_LocationDto_If_Found()
    {
        // Arrange
        var country = await countryRepository.ReadByFilterAsync(c => c.Code == "DK");
        var location = new Location("5000", "Odense", country!.Id, LocationType.ZipCode);
        await locationRepository.CreateAsync(location);

        // Act
        var dto = await locationService.SearchLocationIdAsync("5000", "DK", LocationType.ZipCode);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("5000", dto!.Code);
        Assert.Equal("Odense", dto.Name);
        Assert.Equal("DK", dto.Country.Code);
    }

    [Fact]
    public async Task SearchLocationIdAsync_Returns_Null_If_Location_Not_Found()
    {
        // Act
        var dto = await locationService.SearchLocationIdAsync("9999", "DK", LocationType.ZipCode);

        // Assert
        Assert.Null(dto);
    }

    [Fact]
    public async Task SearchLocationIdAsync_Returns_Null_If_Country_Not_Found()
    {
        // Act
        var dto = await locationService.SearchLocationIdAsync("1000", "XX", LocationType.ZipCode);

        // Assert
        Assert.Null(dto);
    }
}
