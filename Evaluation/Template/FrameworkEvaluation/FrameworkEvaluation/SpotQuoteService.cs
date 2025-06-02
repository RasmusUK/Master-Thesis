namespace FrameworkEvaluation;

public class SpotQuoteService
{
    public SpotQuoteService()
    {
    }
    
    /* Part 1 */
    public async Task<Guid> CreateSpotQuote(string customerName, decimal price)
    {
        throw new NotImplementedException();
    }
    
    public async Task<SpotQuote?> ReadSpotQuote(Guid spotQuoteId)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateSpotQuotePrice(Guid spotQuoteId, decimal price)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteSpotQuote(Guid id)
    {
        throw new NotImplementedException();
    }
    
    /* Part 3 */
    public async Task<SpotQuoteCountriesReadModel?> GetSpotQuoteCountriesReadModel(Guid spotQuoteId)
    {
        throw new NotImplementedException();
    }
    
    /* Part 4 */
    public async Task<Location> GetSpotQuoteOriginLocation(Guid spotQuoteId)
    {
        throw new NotImplementedException();
    }
    
    /* Part 5 */
    public async Task DebugTo(DateTime pointInTime)
    {
        throw new NotImplementedException();
    }
    
    public async Task ResetEntityStore()
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyCollection<SpotQuote>> GetSpotQuoteHistory(Guid spotQuoteId)
    {
        throw new NotImplementedException();
    }
}