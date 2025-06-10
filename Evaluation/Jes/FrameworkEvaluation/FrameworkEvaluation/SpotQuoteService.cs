using EventSourcingFramework.Application.Abstractions.ApiGateway;
using EventSourcingFramework.Application.Abstractions.EntityHistory;
using EventSourcingFramework.Application.Abstractions.Replay;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Stores.EventStore;
using EventSourcingFramework.Core.Interfaces;
using System;

namespace FrameworkEvaluation;

public class SpotQuoteService
{
    public SpotQuoteService(IRepository<SpotQuote> spotquoteRepository, IApiGateway apiGateway, IReplayService replayService, IEntityHistoryService entityHistoryService, IEventStore eventStore)
    {
        _spotquoteRepository=spotquoteRepository;
        _apiGateway=apiGateway;
        _replayService=replayService;
        _entityHistoryService=entityHistoryService;
        _eventStore = eventStore;
    }
    private readonly IRepository<SpotQuote> _spotquoteRepository;
    private readonly IApiGateway _apiGateway;
    private readonly IReplayService _replayService;
    private readonly IEntityHistoryService _entityHistoryService;
    private readonly IEventStore _eventStore;
    /* Part 4 */
    public async Task<Guid> CreateSpotQuote(Guid customerId, double price)
    {
        var spotquote = new SpotQuote(customerId, price);
        await _spotquoteRepository.CreateAsync(spotquote);
        return spotquote.Id;
    }
    
    public async Task<SpotQuote?> GetSpotQuote(Guid spotQuoteId)
    {
        return await _spotquoteRepository.ReadByIdAsync(spotQuoteId);
    }

    public async Task UpdateSpotQuotePrice(Guid spotQuoteId, double price)
    {
        var spot = await _spotquoteRepository.ReadByIdAsync(spotQuoteId);
        spot.Price = price;
        await _spotquoteRepository.UpdateAsync(spot);
    }

    public async Task DeleteSpotQuote(Guid spotQuoteId)
    {
        var car = await _spotquoteRepository.ReadByIdAsync(spotQuoteId);
        await _spotquoteRepository.DeleteAsync(car);
    }
    
    /* Part 5 */
    public async Task<IReadOnlyCollection<Guid>> GetAllSpotQuoteIdsForCustomer(Guid customerId)
    {
        return await _spotquoteRepository.ReadAllProjectionsByFilterAsync
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

    public async Task DebugTo(DateTime pointInTime)
    {
        await _replayService.ReplayUntilAsync(pointInTime,
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
        return (await _eventStore.GetEventsFromAsync(DateTime.UtcNow.AddHours(-1))).Count();
    }
}