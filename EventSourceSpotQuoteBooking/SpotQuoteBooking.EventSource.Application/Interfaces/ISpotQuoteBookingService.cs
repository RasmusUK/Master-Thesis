using SpotQuoteBooking.Shared.Dtos;

namespace SpotQuoteBooking.EventSource.Application.Interfaces;

public interface ISpotQuoteBookingService
{
    Task<Guid> CreateSpotQuoteBookingAsync(CreateSpotQuoteBookingDto createSpotQuoteBookingDto);
    Task<Core.SpotQuoteBooking?> GetSpotQuoteBookingByIdAsync(Guid id);
}
