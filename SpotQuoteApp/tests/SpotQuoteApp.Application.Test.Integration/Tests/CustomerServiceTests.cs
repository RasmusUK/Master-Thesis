using EventSourcingFramework.Core.Interfaces;
using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Application.Interfaces;
using SpotQuoteApp.Core.AggregateRoots;
using SpotQuoteApp.Core.Exceptions;

namespace SpotQuoteApp.Application.Test.Integration.Tests;

[Collection("Integration")]
public class CustomerServiceTests : IAsyncLifetime
{
    private readonly ICustomerService customerService;
    private readonly IRepository<Customer> customerRepository;
    private readonly ISpotQuoteService spotQuoteService;
    private readonly IRepository<SpotQuote> spotQuoteRepository;
    private readonly EntityFactory entityFactory;

    public CustomerServiceTests(
        ICustomerService customerService,
        IRepository<Customer> customerRepository,
        ISpotQuoteService spotQuoteService,
        IRepository<SpotQuote> spotQuoteRepository,
        EntityFactory entityFactory
    )
    {
        this.customerService = customerService;
        this.customerRepository = customerRepository;
        this.spotQuoteService = spotQuoteService;
        this.spotQuoteRepository = spotQuoteRepository;
        this.entityFactory = entityFactory;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        var spotQuotes = await spotQuoteRepository.ReadAllAsync();
        foreach (var quote in spotQuotes)
            await spotQuoteRepository.DeleteAsync(quote);

        var customers = await customerRepository.ReadAllAsync();
        foreach (var customer in customers)
            await customerRepository.DeleteAsync(customer);
    }

    [Fact]
    public async Task AddCustomerAsync_Then_GetCustomerByIdAsync_Returns_Correct_Data()
    {
        // Arrange
        var dto = EntityFactory.CreateCustomerDto("Customer");

        // Act
        await customerService.AddCustomerAsync(dto);
        var stored = await customerService.GetCustomerByIdAsync(dto.Id);

        // Assert
        Assert.NotNull(stored);
        Assert.Equal("Customer", stored!.Name);
    }

    [Fact]
    public async Task GetAllCustomersAsync_Returns_Customers()
    {
        // Arrange
        await customerService.AddCustomerAsync(EntityFactory.CreateCustomerDto("A"));
        await customerService.AddCustomerAsync(EntityFactory.CreateCustomerDto("B"));

        // Act
        var all = await customerService.GetAllCustomersAsync();

        // Assert
        Assert.True(all.Count >= 2);
    }

    [Fact]
    public async Task DeleteCustomerAsync_Succeeds_When_No_SpotQuotes()
    {
        // Arrange
        var dto = EntityFactory.CreateCustomerDto("ToDelete");
        await customerService.AddCustomerAsync(dto);

        // Act
        await customerService.DeleteCustomerAsync(dto);
        var result = await customerService.GetCustomerByIdAsync(dto.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteCustomerAsync_Throws_If_SpotQuotes_Exist()
    {
        // Arrange
        var dto = EntityFactory.CreateCustomerDto("HasQuotes");
        await customerService.AddCustomerAsync(dto);
        var quote = entityFactory.CreateValidSpotQuote();
        quote.Customer = dto;
        await spotQuoteService.CreateSpotQuoteAsync(quote);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => customerService.DeleteCustomerAsync(dto)
        );
        Assert.Contains("Cannot delete customer", ex.Message);
    }

    [Fact]
    public async Task AddUserAsync_Adds_User_To_Customer()
    {
        // Arrange
        var customer = EntityFactory.CreateCustomerDto("WithUser");
        await customerService.AddCustomerAsync(customer);

        var user = new UserDto
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Email = "test@test.com",
        };

        // Act
        await customerService.AddUserAsync(customer.Id, user);
        var stored = await customerService.GetCustomerByIdAsync(customer.Id);

        // Assert
        Assert.Contains(stored!.Users, u => u.Id == user.Id);
    }

    [Fact]
    public async Task DeleteUserAsync_Removes_User_If_No_Quotes()
    {
        // Arrange
        var customer = EntityFactory.CreateCustomerDto("RemoveUser");
        var user = new UserDto
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Email = "test@test.com",
        };
        customer.Users.Add(user);
        await customerService.AddCustomerAsync(customer);

        // Act
        await customerService.DeleteUserAsync(customer.Id, user.Id);
        var stored = await customerService.GetCustomerByIdAsync(customer.Id);

        // Assert
        Assert.DoesNotContain(stored!.Users, u => u.Id == user.Id);
    }

    [Fact]
    public async Task DeleteUserAsync_Throws_If_User_Is_Used_In_SpotQuote()
    {
        // Arrange
        var customer = EntityFactory.CreateCustomerDto("BlockedDelete");
        var user = new UserDto
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Email = "test@test.com",
        };
        customer.Users.Add(user);
        await customerService.AddCustomerAsync(customer);

        var quote = entityFactory.CreateValidSpotQuote();
        quote.Customer = customer;
        quote.MailOptions.UserRecipients.Add(user);
        await spotQuoteService.CreateSpotQuoteAsync(quote);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => customerService.DeleteUserAsync(customer.Id, user.Id)
        );
        Assert.Contains("Cannot delete user", ex.Message);
    }

    [Fact]
    public async Task DeleteUserAsync_Throws_If_User_Not_Found()
    {
        // Arrange
        var customer = EntityFactory.CreateCustomerDto("NoSuchUser");
        await customerService.AddCustomerAsync(customer);
        var badUserId = Guid.NewGuid();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => customerService.DeleteUserAsync(customer.Id, badUserId)
        );
        Assert.Contains("not found in customer", ex.Message);
    }

    [Fact]
    public async Task AddUserAsync_Throws_If_Customer_Not_Found()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();
        var user = new UserDto
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Email = "test@test.com",
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => customerService.AddUserAsync(nonExistentCustomerId, user)
        );

        Assert.Contains($"Customer with id '{nonExistentCustomerId}' not found", ex.Message);
    }

    [Fact]
    public async Task DeleteUserAsync_Throws_If_Customer_Not_Found()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();
        var nonExistentUserId = Guid.NewGuid();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => customerService.DeleteUserAsync(nonExistentCustomerId, nonExistentUserId)
        );

        Assert.Contains($"Customer with id '{nonExistentCustomerId}' not found", ex.Message);
    }
}
