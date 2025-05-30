using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;

namespace EventSourcingFramework.Test.Utilities;

public class MongoIntegrationTestBase : IAsyncLifetime
{
    protected readonly IMongoDbService MongoDbService;
    protected readonly IReplayContext ReplayContext;

    protected MongoIntegrationTestBase(
        IMongoDbService mongoDbService,
        IReplayContext replayContext
    )
    {
        MongoDbService = mongoDbService;
        ReplayContext = replayContext;
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