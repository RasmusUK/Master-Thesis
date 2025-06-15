using EventSourcingFramework.Core.Interfaces;
using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Application.Interfaces;
using SpotQuoteApp.Core.DomainObjects;
using SpotQuoteApp.Core.Exceptions;

namespace SpotQuoteApp.Application.Test.Integration.Tests;

[Collection("Integration")]
public class AddressServiceTests : IAsyncLifetime
{
    private readonly IAddressService addressService;
    private readonly IRepository<Country> countryRepository;
    private readonly IRepository<Address> addressRepository;

    public AddressServiceTests(
        IAddressService addressService,
        IRepository<Country> countryRepository,
        IRepository<Address> addressRepository
    )
    {
        this.addressService = addressService;
        this.countryRepository = countryRepository;
        this.addressRepository = addressRepository;
    }

    public async Task InitializeAsync()
    {
        await DisposeAsync();
        await countryRepository.CreateAsync(new Country("Denmark", "DK"));
    }

    public async Task DisposeAsync()
    {
        var countries = await countryRepository.ReadAllAsync();
        foreach (var country in countries)
            await countryRepository.DeleteAsync(country);

        var addresses = await addressRepository.ReadAllAsync();
        foreach (var address in addresses)
            await addressRepository.DeleteAsync(address);
    }

    [Fact]
    public async Task CreateIfNotExistsAsync_Should_Create_New_Address()
    {
        // Arrange
        var dto = CreateAddressDto();

        // Act
        var id = await addressService.CreateIfNotExistsAsync(dto);

        // Assert
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task CreateIfNotExistsAsync_Should_Return_Same_Id_For_Existing_Address()
    {
        // Arrange
        var dto = CreateAddressDto();

        var firstId = await addressService.CreateIfNotExistsAsync(dto);
        var secondId = await addressService.CreateIfNotExistsAsync(dto);

        // Assert
        Assert.Equal(firstId, secondId);
    }

    [Fact]
    public async Task GetAddressByIdAsync_Should_Return_Correct_Address()
    {
        // Arrange
        var dto = CreateAddressDto();
        var id = await addressService.CreateIfNotExistsAsync(dto);

        // Act
        var result = await addressService.GetAddressByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.CompanyName, result.CompanyName);
        Assert.Equal(dto.Email, result.Email);
    }

    [Fact]
    public async Task GetAllAddressesAsync_Should_Return_At_Least_One()
    {
        // Arrange
        await addressService.CreateIfNotExistsAsync(CreateAddressDto());

        // Act
        var all = await addressService.GetAllAddressesAsync();

        // Assert
        Assert.NotNull(all);
        Assert.True(all.Count > 0);
    }

    [Fact]
    public async Task CreateIfNotExistsAsync_Throws_If_Country_Not_Found_By_Id_Or_Code()
    {
        // Arrange
        var dto = CreateAddressDto();
        dto.Country.Id = Guid.NewGuid();
        dto.Country.Code = "XX";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => addressService.CreateIfNotExistsAsync(dto)
        );

        Assert.Contains("Country with country code 'XX' not found", ex.Message);
    }

    [Fact]
    public async Task GetAddressByIdAsync_Returns_Null_If_Not_Exists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await addressService.GetAddressByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAddressByIdAsync_Throws_If_Country_Not_Found()
    {
        // Arrange
        var fakeCountryId = Guid.NewGuid();
        var address = new Address(
            "Fake",
            fakeCountryId,
            "Fake",
            "Fake",
            "Fake",
            "Fake",
            "Fake",
            "Fake",
            "Fake",
            "Fake"
        );
        await addressRepository.CreateAsync(address);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => addressService.GetAddressByIdAsync(address.Id)
        );

        Assert.Contains($"Country with id '{fakeCountryId}' not found", ex.Message);
    }

    private AddressDto CreateAddressDto()
    {
        return new AddressDto
        {
            Id = Guid.NewGuid(),
            CompanyName = "Company",
            Email = "company@test.com",
            Phone = "12345678",
            Attention = "Test Person",
            Country = new CountryDto
            {
                Code = "DK",
                Name = "Denmark",
                Id = Guid.NewGuid(),
            },
            ZipCode = "1234",
            AddressLine1 = "Main Street",
            AddressLine2 = "Second street",
            City = "Copenhagen",
        };
    }
}
