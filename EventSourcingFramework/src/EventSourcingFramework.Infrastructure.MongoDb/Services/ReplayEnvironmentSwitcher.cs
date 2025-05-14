using EventSourcingFramework.Application.Abstractions.Replay;
using EventSourcingFramework.Infrastructure.Shared.Interfaces;

namespace EventSourcingFramework.Infrastructure.MongoDb.Services;

public class ReplayEnvironmentSwitcher : IReplayEnvironmentSwitcher
{
    private readonly IMongoDbService mongoDbService;

    public ReplayEnvironmentSwitcher(IMongoDbService mongoDbService)
    {
        this.mongoDbService = mongoDbService;
    }

    public Task UseDebugDatabaseAsync()
    {
        mongoDbService.UseDebugEntityDatabase();
        return Task.CompletedTask;
    }

    public Task UseProductionDatabaseAsync()
    {
        mongoDbService.UseProductionEntityDatabase();
        return Task.CompletedTask;
    }
}