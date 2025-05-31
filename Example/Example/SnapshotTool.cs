using EventSourcingFramework.Application.Abstractions.EventStore;
using EventSourcingFramework.Application.Abstractions.Snapshots;

namespace Example;

public class SnapshotTool
{
    private readonly ISnapshotService snapshotService;
    private readonly IEventSequenceGenerator eventSequenceGenerator;

    public SnapshotTool(ISnapshotService snapshotService, IEventSequenceGenerator eventSequenceGenerator)
    {
        this.snapshotService = snapshotService;
        this.eventSequenceGenerator = eventSequenceGenerator;
    }

    public async Task TakeSnapshotAsync()
    {
        var currentEventNumber = await eventSequenceGenerator.GetCurrentSequenceNumberAsync();
        await snapshotService.TakeSnapshotAsync(currentEventNumber);
    }
    
    public async Task RestoreSnapshotAsync(string snapshotId)
    {
        await snapshotService.RestoreSnapshotAsync(snapshotId);
    }
    
    public async Task DeleteSnapshotAsync(string snapshotId)
    {
        await snapshotService.DeleteSnapshotAsync(snapshotId);
    }
}
