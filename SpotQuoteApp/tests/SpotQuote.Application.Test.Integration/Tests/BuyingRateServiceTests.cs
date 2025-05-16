using EventSourcingFramework.Core.Interfaces;
using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Application.Interfaces;
using SpotQuoteApp.Core.AggregateRoots;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuote.Application.Test.Integration.Tests;

public class BuyingRateServiceTests : IAsyncLifetime
{
    private readonly IBuyingRateService buyingRateService;
    private readonly IRepository<BuyingRate> buyingRateRepository;
    private readonly IRepository<Location> locationRepository;
    private readonly IRepository<Country> countryRepository;

    public BuyingRateServiceTests(
        IBuyingRateService buyingRateService,
        IRepository<BuyingRate> buyingRateRepository,
        IRepository<Location> locationRepository, IRepository<Country> countryRepository)
    {
        this.buyingRateService = buyingRateService;
        this.buyingRateRepository = buyingRateRepository;
        this.locationRepository = locationRepository;
        this.countryRepository = countryRepository;
        countryRepository.CreateAsync(new Country("Denmark", "DK")).GetAwaiter().GetResult();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        var allRates = await buyingRateRepository.ReadAllAsync();
        foreach (var rate in allRates)
            await buyingRateRepository.DeleteAsync(rate);

        var locations = await locationRepository.ReadAllAsync();
        foreach (var loc in locations)
            await locationRepository.DeleteAsync(loc);
        
        var countries = await countryRepository.ReadAllAsync();
        foreach (var country in countries)
            await countryRepository.DeleteAsync(country);
    }

    [Fact]
    public async Task UpsertBuyingRatesAsync_Should_Insert_New_Rate()
    {
        // Arrange
        var spotQuote = EntityFactory.CreateValidSpotQuote();

        // Act
        await buyingRateService.UpsertBuyingRatesAsync(spotQuote);

        // Assert
        var all = await buyingRateRepository.ReadAllAsync();
        Assert.Single(all);
        Assert.Equal(spotQuote.Quotes.First().Supplier, all.First().Supplier);
    }

    [Fact]
    public async Task UpsertBuyingRatesAsync_Should_Update_If_Newer_ValidUntil()
    {
        // Arrange
        var spotQuote = EntityFactory.CreateValidSpotQuote();
        await buyingRateService.UpsertBuyingRatesAsync(spotQuote);

        spotQuote.ValidUntil = spotQuote.ValidUntil!.Value.AddDays(10);

        // Act
        await buyingRateService.UpsertBuyingRatesAsync(spotQuote);

        // Assert
        var rate = (await buyingRateRepository.ReadAllAsync()).First();
        Assert.Equal(spotQuote.ValidUntil.Value.DayOfYear, rate.ValidUntil.DayOfYear);
    }
    
    [Fact]
    public async Task SearchBuyingRatesAsync_Should_Return_Stored_Rate()
    {
        // Arrange
        var spotQuote = EntityFactory.CreateValidSpotQuote();
        await buyingRateService.UpsertBuyingRatesAsync(spotQuote);

        var addressFrom = spotQuote.AddressFrom;
        var addressTo = spotQuote.AddressTo;
        var quote = spotQuote.Quotes.First();

        // Act
        var result = await buyingRateService.SearchBuyingRatesAsync(
            addressFrom,
            addressTo,
            spotQuote.TransportMode,
            quote.Supplier,
            quote.SupplierService,
            quote.ForwarderService
        );

        // Assert
        Assert.NotNull(result);
        var rate = result.FirstOrDefault(q => q.Id != Guid.Empty);
        Assert.NotNull(rate);

        Assert.Equal(quote.Supplier, rate.Supplier);
        Assert.Equal(quote.SupplierService, rate.SupplierService);
        Assert.Equal(quote.ForwarderService, rate.ForwarderService);
        Assert.Equal(addressFrom.ZipCode, rate.Origin.Code);
        Assert.Equal(addressTo.ZipCode, rate.Destination.Code);
    }

    [Fact]
    public async Task SearchBuyingRatesAsync_Should_Return_No_Stored_Rates_If_No_Match()
    {
        // Arrange
        var spotQuote = EntityFactory.CreateValidSpotQuote();
        await buyingRateService.UpsertBuyingRatesAsync(spotQuote);

        // Change address so that no match is found
        var unmatchedFrom = new AddressDto
        {
            City = "Oslo",
            Country = new CountryDto { Code = "NO", Name = "Norway", Id = Guid.NewGuid() },
            ZipCode = "0010"
        };

        var unmatchedTo = new AddressDto
        {
            City = "Stockholm",
            Country = new CountryDto { Code = "SE", Name = "Sweden", Id = Guid.NewGuid() },
            ZipCode = "10000"
        };

        // Act
        var result = await buyingRateService.SearchBuyingRatesAsync(
            unmatchedFrom,
            unmatchedTo,
            spotQuote.TransportMode,
            Supplier.DHL,
            SupplierService.DHLExpress12,
            ForwarderService.DHLExpress
        );

        // Assert
        Assert.NotNull(result);
        var rate = result.FirstOrDefault(q => q.Id != Guid.Empty);
        Assert.Null(rate);
    }
}