using EventSourcingFramework.Application.Abstractions.EntityHistory;
using EventSourcingFramework.Application.Abstractions.Replay;
using EventSourcingFramework.Core.Models.Entity;

namespace Example;

public class DebugService
{
    private readonly IReplayService replayService;
    private readonly IEntityHistoryService entityHistoryService;

    public DebugService(IReplayService replayService, IEntityHistoryService entityHistoryService)
    {
        this.replayService = replayService;
        this.entityHistoryService = entityHistoryService;
    }
    
    public Task TimeTravelToAsync(DateTime dateTime)
    {
        return replayService.ReplayUntilAsync(dateTime);
    }
    
    public Task RestoreEntityStoreAsync()
    {
        return replayService.ReplayAllAsync(useSnapshot:false);
    }
    
    public Task<IReadOnlyCollection<T>> GetEntityHistoryAsync<T>(Guid entityId) where T : Entity
    {
        return entityHistoryService.GetEntityHistoryAsync<T>(entityId);
    }
}