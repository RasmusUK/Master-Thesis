using EventSourcingFramework.Application.Interfaces;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Persistence;
using EventSourcingFramework.Persistence.Events;
using EventSourcingFramework.Persistence.Interfaces;
using EventSourcingFramework.Test.Utilities;
using Moq;

namespace EventSourcingFramework.Application.Test.Integration;

[Collection("Integration")]
public class PersonalDataServiceTests : MongoIntegrationTestBase
{
    private readonly IPersonalDataService personalDataService;
    private readonly IPersonalDataStore personalDataStore;

    public PersonalDataServiceTests(
        IMongoDbService mongoDbService,
        IGlobalReplayContext globalReplayContext,
        IPersonalDataService personalDataService,
        IPersonalDataStore personalDataStore
    )
        : base(mongoDbService, globalReplayContext)
    {
        this.personalDataService = personalDataService;
        this.personalDataStore = personalDataStore;
    }

    [Fact]
    public async Task StripAndStoreAsync_StripsTopLevelAndNestedProperties()
    {
        // Arrange
        var entity = new PersonEntity
        {
            Id = Guid.NewGuid(),
            Name = "Alice",
            Email = "alice@example.com",
            Address = new Address
            {
                Street = "Main St",
                City = "Copenhagen",
                Location = new Location { Latitude = 55.6761m, Longitude = 12.5683m },
            },
        };

        var evt = new PersonCreatedEvent(entity);

        // Act
        await personalDataService.StripAndStoreAsync(evt);
        var stored = await personalDataStore.RetrieveAsync(evt.Id);

        // Assert
        Assert.Null(evt.Entity.Name);
        Assert.Null(evt.Entity.Email);
        Assert.Null(evt.Entity.Address.Street);
        Assert.Null(evt.Entity.Address.City);
        Assert.Equal(0, evt.Entity.Address.Location.Latitude);
        Assert.Equal(0, evt.Entity.Address.Location.Longitude);

        // Assert
        Assert.Equal("Alice", stored["Name"]);
        Assert.Equal("alice@example.com", stored["Email"]);
        Assert.Equal("Main St", stored["Address.Street"]);
        Assert.Equal("Copenhagen", stored["Address.City"]);
        Assert.Equal(55.6761m, stored["Address.Location.Latitude"]);
        Assert.Equal(12.5683m, stored["Address.Location.Longitude"]);
    }

    [Fact]
    public async Task RestoreAsync_RestoresAllStrippedValues()
    {
        // Arrange
        var id = Guid.NewGuid();

        await personalDataStore.StoreAsync(
            id,
            new Dictionary<string, object?>
            {
                { "Name", "Alice" },
                { "Email", "alice@example.com" },
                { "Address.Street", "Main St" },
                { "Address.City", "Copenhagen" },
                { "Address.Location.Latitude", 55.6761m },
                { "Address.Location.Longitude", 12.5683m },
            }
        );

        var entity = new PersonEntity
        {
            Id = id,
            Name = null,
            Email = null,
            Address = new Address { Location = new Location() },
        };

        var evt = new PersonCreatedEvent(entity) { Id = id };

        // Act
        await personalDataService.RestoreAsync(evt);

        // Assert
        Assert.Equal("Alice", evt.Entity.Name);
        Assert.Equal("alice@example.com", evt.Entity.Email);
        Assert.Equal("Main St", evt.Entity.Address.Street);
        Assert.Equal("Copenhagen", evt.Entity.Address.City);
        Assert.Equal(55.6761m, evt.Entity.Address.Location.Latitude);
        Assert.Equal(12.5683m, evt.Entity.Address.Location.Longitude);
    }

    [Fact]
    public async Task StripAndStoreAsync_DoesNotStoreIfNoPersonalData()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Generic" };
        var evt = new CreateEvent<TestEntity>(entity);

        // Act
        await personalDataService.StripAndStoreAsync(evt);
        var result = await personalDataStore.RetrieveAsync(evt.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task RestoreAsync_HandlesMissingPersonalData()
    {
        // Arrange
        var id = Guid.NewGuid();

        await personalDataStore.StoreAsync(
            id,
            new Dictionary<string, object?>
            {
                { "Name", null },
                { "Email", null },
                { "Address.Location.Latitude", null },
                { "Address.Location.Longitude", null },
            }
        );

        var entity = new PersonEntity
        {
            Id = id,
            Name = null,
            Email = null,
            Address = new Address
            {
                Street = null,
                City = null,
                Location = new Location(),
            },
        };

        var evt = new PersonCreatedEvent(entity) { Id = id };

        // Act
        await personalDataService.RestoreAsync(evt);

        // Assert
        Assert.Null(evt.Entity.Name);
        Assert.Null(evt.Entity.Email);
        Assert.Null(evt.Entity.Address.Street);
        Assert.Null(evt.Entity.Address.City);
        Assert.Equal(0, evt.Entity.Address.Location.Latitude);
        Assert.Equal(0, evt.Entity.Address.Location.Longitude);
    }
}
