using EventSource.Persistence.Interfaces;

namespace EventSource.Test.Utilities;

public class MongoIntegrationTestBase : IAsyncLifetime
{
    private readonly IMongoDbService mongoDbService;

    protected MongoIntegrationTestBase(IMongoDbService mongoDbService)
    {
        this.mongoDbService = mongoDbService;
    }

    public async Task InitializeAsync() => await mongoDbService.CleanUpAsync();

    public async Task DisposeAsync() => await mongoDbService.CleanUpAsync();
}
