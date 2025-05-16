using EventSourcingFramework.Core.Interfaces;
using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Application.Interfaces;
using SpotQuoteApp.Core.AggregateRoots;

namespace SpotQuote.Application.Test.Integration.Tests;

public class AddressServiceTests : IAsyncLifetime
{
    private readonly IAddressService addressService;
    private readonly IRepository<Country> countryRepository;
    private readonly IRepository<Address> addressRepository;

    public AddressServiceTests(IAddressService addressService, IRepository<Country> countryRepository, IRepository<Address> addressRepository)
    {
        this.addressService = addressService;
        this.countryRepository = countryRepository;
        this.addressRepository = addressRepository;
        countryRepository.CreateAsync(new Country("Denmark", "DK")).GetAwaiter().GetResult();
    }


    public Task InitializeAsync() => Task.CompletedTask;

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
        var dto = CreateAddressDto("UniqueCompany");

        // Act
        var id = await addressService.CreateIfNotExistsAsync(dto);

        // Assert
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task CreateIfNotExistsAsync_Should_Return_Same_Id_For_Existing_Address()
    {
        // Arrange
        var dto = CreateAddressDto("DuplicateCompany");

        var firstId = await addressService.CreateIfNotExistsAsync(dto);
        var secondId = await addressService.CreateIfNotExistsAsync(dto);

        // Assert
        Assert.Equal(firstId, secondId);
    }

    [Fact]
    public async Task GetAddressByIdAsync_Should_Return_Correct_Address()
    {
        // Arrange
        var dto = CreateAddressDto("FindMeCorp");
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
        await addressService.CreateIfNotExistsAsync(CreateAddressDto("AllCorp"));

        // Act
        var all = await addressService.GetAllAddressesAsync();

        // Assert
        Assert.NotNull(all);
        Assert.True(all.Count > 0);
    }

    private AddressDto CreateAddressDto(string companyName)
    {
        return new AddressDto
        {
            Id = Guid.NewGuid(),
            CompanyName = companyName,
            Email = $"{companyName.ToLower()}@test.com",
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
            AddressLine2 = "Suite 2",
            City = "Copenhagen"
        };
    }
}