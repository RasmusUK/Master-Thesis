using SpotQuoteBooking.EventSource.Application.DTOs;
using SpotQuoteBooking.EventSource.Core.AggregateRoots;

namespace SpotQuoteBooking.EventSource.Application.Interfaces;

public interface ISpotQuoteService
{
    Task CreateSpotQuoteAsync(SpotQuoteDto spotQuote);
    Task UpdateSpotQuoteAsync(SpotQuoteDto spotQuote);
    Task<SpotQuoteDto?> GetSpotQuoteByIdAsync(Guid id);
    Task<IReadOnlyCollection<SpotQuoteDto>> GetAllSpotQuotesAsync();
}
