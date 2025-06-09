namespace FrameworkEvaluation;

public class SpotQuoteService
{
    public SpotQuoteService()
    {
    }
    
    /* Part 4 */
    public async Task<Guid> CreateSpotQuote(Guid customerId, double price)
    {
        throw new NotImplementedException();
    }
    
    public async Task<SpotQuote?> GetSpotQuote(Guid spotQuoteId)
    {        
        throw new NotImplementedException();
    }

    public async Task UpdateSpotQuotePrice(Guid spotQuoteId, double price)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteSpotQuote(Guid spotQuoteId)
    {
        throw new NotImplementedException();
    }
    
    /* Part 5 */
    public async Task<IReadOnlyCollection<Guid>> GetAllSpotQuoteIdsForCustomer(Guid customerId)
    {
        throw new NotImplementedException();
    }
    
    /* Part 6 */
    public async Task<IReadOnlyCollection<SpotQuote>> FetchExternalSpotQuotes()
    {        
        throw new NotImplementedException();
    }

    /* Part 7 */
    public async Task<IReadOnlyCollection<SpotQuote>> GetSpotQuoteHistory(Guid spotQuoteId)
    {
        throw new NotImplementedException();
    }

    public async Task DebugTo(DateTime pointInTime)
    {
        throw new NotImplementedException();
    }
    
    public async Task ResetEntityStore()
    {
        throw new NotImplementedException();
    }

    /* Part 10 */
    public async Task<int> NrOfEventsTheLastHour()
    {
        throw new NotImplementedException();
    }
}