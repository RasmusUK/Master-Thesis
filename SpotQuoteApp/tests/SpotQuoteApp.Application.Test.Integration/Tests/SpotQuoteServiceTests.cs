using EventSourcingFramework.Core.Interfaces;
using SpotQuoteApp.Application.Interfaces;
using SpotQuoteApp.Application.Mappers;
using SpotQuoteApp.Core.DomainObjects;
using SpotQuoteApp.Core.Exceptions;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Application.Test.Integration.Tests;

[Collection("Integration")]
public class SpotQuoteServiceTests : IAsyncLifetime
{
    private readonly ISpotQuoteService spotQuoteService;
    private readonly IRepository<SpotQuote> spotQuoteRepository;
    private readonly IRepository<Address> addressRepository;
    private readonly IRepository<Customer> customerRepository;
    private readonly EntityFactory entityFactory;

    public SpotQuoteServiceTests(
        ISpotQuoteService spotQuoteService,
        IRepository<SpotQuote> spotQuoteRepository,
        EntityFactory entityFactory,
        IRepository<Address> addressRepository,
        IRepository<Customer> customerRepository
    )
    {
        this.spotQuoteService = spotQuoteService;
        this.spotQuoteRepository = spotQuoteRepository;
        this.entityFactory = entityFactory;
        this.addressRepository = addressRepository;
        this.customerRepository = customerRepository;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        var all = await spotQuoteRepository.ReadAllAsync();
        foreach (var quote in all)
            await spotQuoteRepository.DeleteAsync(quote);
    }

    [Fact]
    public async Task CreateSpotQuoteAsync_Should_Create_Successfully()
    {
        // Arrange
        var dto = entityFactory.CreateValidSpotQuote();

        // Act
        await spotQuoteService.CreateSpotQuoteAsync(dto);
        var stored = await spotQuoteService.GetSpotQuoteByIdAsync(dto.Id);

        // Assert
        Assert.NotNull(stored);
        Assert.Equal(dto.TransportMode, stored!.TransportMode);
        Assert.Equal(dto.AddressFrom.City, stored.AddressFrom.City);
    }

    [Fact]
    public async Task UpdateSpotQuoteAsync_Should_Update_Successfully()
    {
        // Arrange
        var dto = entityFactory.CreateValidSpotQuote();
        await spotQuoteService.CreateSpotQuoteAsync(dto);

        // Act
        dto.ShippingDetails.Description = "Updated Description";
        await spotQuoteService.UpdateSpotQuoteAsync(dto);
        var result = await spotQuoteService.GetSpotQuoteByIdAsync(dto.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Description", result!.ShippingDetails.Description);
    }

    [Fact]
    public async Task GetSpotQuoteByIdAsync_Returns_Null_If_Not_Exists()
    {
        // Act
        var result = await spotQuoteService.GetSpotQuoteByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllSpotQuotesAsync_Returns_List()
    {
        // Arrange
        await spotQuoteService.CreateSpotQuoteAsync(entityFactory.CreateValidSpotQuote());
        await spotQuoteService.CreateSpotQuoteAsync(entityFactory.CreateValidSpotQuote());

        // Act
        var all = await spotQuoteService.GetAllSpotQuotesAsync();

        // Assert
        Assert.NotEmpty(all);
        Assert.True(all.Count >= 2);
    }

    [Fact]
    public async Task CreateSpotQuoteAsync_Throws_If_Customer_Not_Exists()
    {
        // Arrange
        var dto = entityFactory.CreateValidSpotQuote();
        dto.Customer.Id = Guid.NewGuid();
        dto.Quotes.First().Status = BookingStatus.SpotQuote;

        // Act & Assert
        var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => spotQuoteService.CreateSpotQuoteAsync(dto)
        );

        Assert.Contains("Customer does not exist", ex.Message);
    }

    [Fact]
    public async Task CreateSpotQuoteAsync_Throws_If_Address_Not_Exists()
    {
        // Arrange
        var dto = entityFactory.CreateValidSpotQuote();
        dto.AddressFrom.Country.Code = "XX";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => spotQuoteService.CreateSpotQuoteAsync(dto)
        );

        Assert.Contains("Country with code", ex.Message);
    }

    [Fact]
    public async Task CreateSpotQuoteAsync_Throws_If_Invalid_Data()
    {
        // Arrange
        var dto = entityFactory.CreateValidSpotQuote();
        dto.ShippingDetails.Collis.Clear();
        dto.Quotes.First().Status = BookingStatus.SpotQuote;

        // Act & Assert
        var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => spotQuoteService.CreateSpotQuoteAsync(dto)
        );

        Assert.Contains("at least one", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetSpotQuoteByIdAsync_Throws_If_Customer_Not_Found()
    {
        // Arrange
        var dto = entityFactory.CreateValidSpotQuote();
        await spotQuoteService.CreateSpotQuoteAsync(dto);

        // Manually delete the customer (simulate foreign key break)
        var customer = dto.Customer;
        await customerRepository.DeleteAsync(customer.ToDomain());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => spotQuoteService.GetSpotQuoteByIdAsync(dto.Id)
        );

        Assert.Contains($"Customer with id '{customer.Id}' not found", ex.Message);
    }

    [Fact]
    public async Task GetSpotQuoteByIdAsync_Throws_If_AddressFrom_Not_Found()
    {
        // Arrange
        var dto = entityFactory.CreateValidSpotQuote();
        await spotQuoteService.CreateSpotQuoteAsync(dto);

        // Delete the from address
        await addressRepository.DeleteAsync(dto.AddressFrom.ToDomain());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => spotQuoteService.GetSpotQuoteByIdAsync(dto.Id)
        );

        Assert.Contains($"Address with id '{dto.AddressFrom.Id}' not found", ex.Message);
    }

    [Fact]
    public async Task GetSpotQuoteByIdAsync_Throws_If_AddressTo_Not_Found()
    {
        // Arrange
        var dto = entityFactory.CreateValidSpotQuote();
        await spotQuoteService.CreateSpotQuoteAsync(dto);

        // Delete the to address
        await addressRepository.DeleteAsync(dto.AddressTo.ToDomain());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => spotQuoteService.GetSpotQuoteByIdAsync(dto.Id)
        );

        Assert.Contains($"Address with id '{dto.AddressTo.Id}' not found", ex.Message);
    }
}
