using EventSource.Core.Interfaces;
using SpotQuoteBooking.EventSource.Application.Interfaces;
using SpotQuoteBooking.EventSource.Core;
using SpotQuoteBooking.Shared;
using SpotQuoteBooking.Shared.Dtos;

namespace SpotQuoteBooking.EventSource.Application;

public class SpotQuoteBookingService : ISpotQuoteBookingService
{
    private readonly IEventProcessor eventProcessor;
    private readonly IEntityStore entityStore;

    public SpotQuoteBookingService(IEventProcessor eventProcessor, IEntityStore entityStore)
    {
        this.eventProcessor = eventProcessor;
        this.entityStore = entityStore;
    }

    public async Task<Guid> CreateSpotQuoteBookingAsync(CreateSpotQuoteBookingDto b)
    {
        var createBookingEvent = new CreateSpotQuoteBookingEvent(
            b.AddressFrom.ToAddress(),
            b.AddressTo.ToAddress(),
            Direction.GetDirection(b.Direction),
            TransportMode.GetTransportMode(b.TransportMode),
            Incoterm.GetIncoterm(b.Incoterm),
            b.ShippingDetails.ToShippingDetails()
        );

        var id = (await eventProcessor.ProcessAsync(createBookingEvent)).Id;
        return id;
    }

    public Task<Core.SpotQuoteBooking?> GetSpotQuoteBookingByIdAsync(Guid id) =>
        entityStore.GetEntityByIdAsync<Core.SpotQuoteBooking>(id);
}
