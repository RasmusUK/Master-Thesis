using EventSourcingFramework.Application.Abstractions;
using EventSourcingFramework.Infrastructure.Abstractions.MongoDb;

namespace EventSourcingFramework.Infrastructure.MongoDb;

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