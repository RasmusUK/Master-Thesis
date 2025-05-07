using EventSource.Application.Interfaces;
using EventSource.Persistence.Interfaces;

namespace EventSource.Test.Utilities;

public class MongoIntegrationTestBase : IAsyncLifetime
{
    protected readonly IMongoDbService MongoDbService;
    protected readonly IGlobalReplayContext ReplayContext;

    protected MongoIntegrationTestBase(
        IMongoDbService mongoDbService,
        IGlobalReplayContext replayContext
    )
    {
        this.MongoDbService = mongoDbService;
        this.ReplayContext = replayContext;
    }

    public async Task InitializeAsync()
    {
        if (ReplayContext.IsReplaying)
            ReplayContext.StopReplay();

        await MongoDbService.CleanUpAsync();
        await MongoDbService.UseProductionEntityDatabase();
    }

    public async Task DisposeAsync()
    {
        if (ReplayContext.IsReplaying)
            ReplayContext.StopReplay();

        await MongoDbService.CleanUpAsync();
        await MongoDbService.UseProductionEntityDatabase();
    }
}
