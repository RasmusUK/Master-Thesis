using EventSource.Application.Interfaces;
using EventSource.Persistence.Interfaces;

namespace EventSource.Test.Utilities;

public class MongoIntegrationTestBase : IAsyncLifetime
{
    private readonly IMongoDbService mongoDbService;
    private readonly IGlobalReplayContext replayContext;

    protected MongoIntegrationTestBase(
        IMongoDbService mongoDbService,
        IGlobalReplayContext replayContext
    )
    {
        this.mongoDbService = mongoDbService;
        this.replayContext = replayContext;
    }

    public async Task InitializeAsync()
    {
        if (replayContext.IsReplaying)
            replayContext.StopReplay();

        await mongoDbService.CleanUpAsync();
        await mongoDbService.UseProductionEntityDatabase();
    }

    public async Task DisposeAsync()
    {
        if (replayContext.IsReplaying)
            replayContext.StopReplay();

        await mongoDbService.CleanUpAsync();
        await mongoDbService.UseProductionEntityDatabase();
    }
}
