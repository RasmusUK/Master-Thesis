using EventSourcingFramework.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Example;

public class AuditLogger
{
    private readonly IEventStore eventStore;
    private readonly ILogger<AuditLogger> logger;

    public AuditLogger(IEventStore eventStore, ILogger<AuditLogger> logger)
    {
        this.eventStore = eventStore;
        this.logger = logger;
    }

    public async Task LogRecentEventsAsync(int count = 50)
    {
        var events = await eventStore.GetEventsAsync();
        var recentEvents = events
            .OrderByDescending(e => e.Timestamp)
            .Take(count);

        foreach (var e in recentEvents)
        {
            logger.LogInformation("Event: {Type} | Event Id: {EventId} | Entity Id: {EntityId} | Timestamp: {Timestamp}",
                e.Typename,
                e.Id,
                e.EntityId,
                e.Timestamp);
        }
    }
}
