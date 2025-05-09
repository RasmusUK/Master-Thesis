using EventSourcingFramework.Application.Interfaces;
using EventSourcingFramework.Persistence.Interfaces;
using EventSourcingFramework.Test.Utilities;

namespace EventSourcingFramework.Persistence.Test.Integration;

[Collection("Integration")]
public class EntityCollectionNameProviderTests : MongoIntegrationTestBase
{
    private readonly EntityCollectionNameProvider provider = new(new MigrationTypeRegistry());

    private class TestEntity1 { }

    private class TestEntity2 { }

    public EntityCollectionNameProviderTests(
        IMongoDbService mongoDbService,
        IGlobalReplayContext replayContext
    )
        : base(mongoDbService, replayContext) { }

    [Fact]
    public void Register_Then_GetCollectionName_ReturnsCorrectValue()
    {
        // Arrange
        var type = typeof(TestEntity1);
        var expectedCollection = "TestCollection";

        // Act
        provider.Register(type, expectedCollection);
        var actual = provider.GetCollectionName(type);

        // Assert
        Assert.Equal(expectedCollection, actual);
    }

    [Fact]
    public void Register_NullType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => provider.Register(null, "Collection"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_EmptyOrWhitespaceCollectionName_ThrowsArgumentException(string name)
    {
        Assert.Throws<ArgumentException>(() => provider.Register(typeof(TestEntity1), name));
    }

    [Fact]
    public void GetCollectionName_UnregisteredType_ThrowsInvalidOperationException()
    {
        var type = typeof(TestEntity2);

        Assert.Throws<InvalidOperationException>(() => provider.GetCollectionName(type));
    }

    [Fact]
    public void GetCollectionName_NullType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => provider.GetCollectionName(null));
    }

    [Fact]
    public void GetAllRegistered_ReturnsAllRegisteredTypes()
    {
        // Arrange
        provider.Register(typeof(TestEntity1), "Test1");
        provider.Register(typeof(TestEntity2), "Test2");

        // Act
        var all = provider.GetAllRegistered();

        // Assert
        Assert.Equal(2, all.Count);
        Assert.Contains(all, x => x.Type == typeof(TestEntity1) && x.CollectionName == "Test1");
        Assert.Contains(all, x => x.Type == typeof(TestEntity2) && x.CollectionName == "Test2");
    }
}
