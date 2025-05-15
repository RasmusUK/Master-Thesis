using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;
using EventSourcingFramework.Test.Utilities;

namespace EventSourcingFramework.Infrastructure.Test.Integration.Stores;

[Collection("Integration")]
public class PersonalDataStoreTests : MongoIntegrationTestBase
{
    private readonly IPersonalDataStore personalDataStore;

    public PersonalDataStoreTests(
        IMongoDbService mongoDbService,
        IReplayContext replayContext,
        IPersonalDataStore personalDataStore
    )
        : base(mongoDbService, replayContext)
    {
        this.personalDataStore = personalDataStore;
    }

    [Fact]
    public async Task StoreAsync_SavesPersonalData()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var data = new Dictionary<string, object?>
        {
            { "Email", "user@example.com" },
            { "SSN", "123-45-6789" }
        };

        // Act
        await personalDataStore.StoreAsync(eventId, data);
        var result = await personalDataStore.RetrieveAsync(eventId);

        // Assert
        Assert.Equal("user@example.com", result["Email"]);
        Assert.Equal("123-45-6789", result["SSN"]);
    }

    [Fact]
    public async Task StoreAsync_OverwritesExistingData()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        var first = new Dictionary<string, object?> { { "Phone", "12345678" } };

        var updated = new Dictionary<string, object?>
        {
            { "Phone", "99999999" },
            { "Zip", "0000" }
        };

        // Act
        await personalDataStore.StoreAsync(eventId, first);
        await personalDataStore.StoreAsync(eventId, updated);
        var result = await personalDataStore.RetrieveAsync(eventId);

        // Assert
        Assert.Equal("99999999", result["Phone"]);
        Assert.Equal("0000", result["Zip"]);
    }

    [Fact]
    public async Task RetrieveAsync_ReturnsEmptyDictionary_WhenNotFound()
    {
        // Arrange
        var unknownEventId = Guid.NewGuid();

        // Act
        var result = await personalDataStore.RetrieveAsync(unknownEventId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}