using EventSource.Core.Interfaces;

namespace EventSource.Core.Test;

public class CreateBookingEventHandler : IEventHandler<CreateBookingEvent>
{
    private readonly IEntityStore entityStore;

    public CreateBookingEventHandler(IEntityStore entityStore)
    {
        this.entityStore = entityStore;
    }

    public async Task HandleAsync(CreateBookingEvent e)
    {
        var booking = await entityStore.GetEntityAsync<Booking>(e.EntityId);
        booking.Name = "Booking";
        await entityStore.SaveEntityAsync(booking);
    }
}
