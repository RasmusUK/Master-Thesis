using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.AggregateRoots;

namespace SpotQuoteApp.Application.Interfaces;

public interface ISpotQuoteService
{
    Task CreateSpotQuoteAsync(SpotQuoteDto spotQuote);
    Task UpdateSpotQuoteAsync(SpotQuoteDto spotQuote);
    Task<SpotQuoteDto?> GetSpotQuoteByIdAsync(Guid id);
    Task<IReadOnlyCollection<SpotQuoteDto>> GetAllSpotQuotesAsync();
}
