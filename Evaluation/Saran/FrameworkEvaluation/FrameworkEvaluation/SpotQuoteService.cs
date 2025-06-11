using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Application.Abstractions.ApiGateway;
using EventSourcingFramework.Application.Abstractions.EntityHistory;
using EventSourcingFramework.Application.Abstractions.Replay;
using EventSourcingFramework.Infrastructure.Stores.EventStore;

namespace FrameworkEvaluation;

public class SpotQuoteService
{
    private readonly IRepository<SpotQuote> _spotQuote;
    private readonly IApiGateway _apiGateway;
    private readonly IReplayService _replayService;
    private readonly IEntityHistoryService _entityHistoryService;
    private readonly IEventStore _eventStore;

    public SpotQuoteService(IRepository<SpotQuote> spotQuote, IApiGateway apiGateway, IReplayService replayService, IEntityHistoryService entityHistoryService, IEventStore eventStore)
    {
        _spotQuote = spotQuote;
        _apiGateway = apiGateway;
        _replayService = replayService;
        _entityHistoryService = entityHistoryService;
        _eventStore = eventStore;
    }
    
    /* Part 4 */
    public async Task<Guid> CreateSpotQuote(Guid customerId, decimal price)
    {
        var spot = new SpotQuote();
        spot.CustomerId = customerId;
        spot.Price = price;
        await _spotQuote.CreateAsync(spot);
        return spot.Id;
    }
    
    public async Task<SpotQuote?> GetSpotQuote(Guid spotQuoteId)
    {
        return await _spotQuote.ReadByIdAsync(spotQuoteId);
    }

    public async Task UpdateSpotQuotePrice(Guid spotQuoteId, decimal price)
    {
        var spot = await _spotQuote.ReadByIdAsync(spotQuoteId);
        spot.Price = price;
        await _spotQuote.UpdateAsync(spot);
    }

    public async Task DeleteSpotQuote(Guid spotQuoteId)
    {
        var spot = await _spotQuote.ReadByIdAsync(spotQuoteId);
        await _spotQuote.DeleteAsync(spot);
    }
    
    /* Part 5 */
    public async Task<IReadOnlyCollection<Guid>> GetAllSpotQuoteIdsForCustomer(Guid customerId)
    {
        return await _spotQuote.ReadAllProjectionsByFilterAsync
           (
             spot => spot.Id, //Projection
             spot => spot.CustomerId == customerId //Filter
           );
    }
    
    /* Part 6 */
    public async Task<IReadOnlyCollection<SpotQuote>> FetchExternalSpotQuotes()
    {
        return await _apiGateway.GetAsync<IReadOnlyCollection<SpotQuote>>("/spotquotes");
    }

    /* Part 7 */
    public async Task<IReadOnlyCollection<SpotQuote>> GetSpotQuoteHistory(Guid spotQuoteId)
    {
        return await _entityHistoryService.GetEntityHistoryAsync<SpotQuote>(spotQuoteId);
    }

    public async Task DebugTo(DateTime dateTime)
    {
        await _replayService.ReplayUntilAsync(dateTime,
          useSnapshot: true,
          autoStop: false
        );
    }


    public async Task ResetEntityStore()
    {
        await _replayService.ReplayAllAsync(
          useSnapshot: false,
          autoStop: true
        );
    }

    /* Part 10 */
    public async Task<int> NrOfEventsTheLastHour()
    {
        var events = await _eventStore.GetEventsFromUntilAsync(DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1));
        return events.Count;
    }
}