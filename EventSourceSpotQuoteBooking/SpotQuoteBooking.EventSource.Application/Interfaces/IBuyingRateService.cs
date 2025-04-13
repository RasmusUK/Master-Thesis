using SpotQuoteBooking.EventSource.Application.DTOs;

namespace SpotQuoteBooking.EventSource.Application.Interfaces;

public interface IBuyingRateService
{
    Task UpsertBuyingRatesAsync(SpotQuoteDto spotQuoteDto);
}
