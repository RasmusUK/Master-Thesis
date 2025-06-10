namespace FrameworkEvaluation;

using EventSourcingFramework.Application.Abstractions.ApiGateway;
using EventSourcingFramework.Application.Abstractions.EntityHistory;
using EventSourcingFramework.Application.Abstractions.Replay;
using EventSourcingFramework.Application.UseCases.EntityHistory;
using EventSourcingFramework.Application.UseCases.Replay;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Http.Services;
using System;

public class SpotQuoteService
{
    private readonly IRepository<SpotQuote> spotQuoteRepository;
    private readonly IApiGateway apiGateway;
    private readonly IEntityHistoryService entityHistoryService;
    private readonly IReplayService replayService;
    private readonly IEventStore eventStore;

    public SpotQuoteService(IRepository<SpotQuote> spotQuoteRepository, IApiGateway apiGateway, IEntityHistoryService entityHistoryService, IReplayService replayService, IEventStore eventStore)
    {
        this.spotQuoteRepository = spotQuoteRepository;
        this.apiGateway = apiGateway;
        this.entityHistoryService = entityHistoryService;
        this.replayService = replayService;
        this.eventStore = eventStore;
    }

    /* Part 4 */
    public async Task<Guid> CreateSpotQuote(Guid customerId, double price)
    {
        var spotQuote = new SpotQuote()
        {
            CustomerId = customerId,
            Price = price
        };
        await spotQuoteRepository.CreateAsync(spotQuote);
        return spotQuote.Id;
    }

    public async Task<SpotQuote?> GetSpotQuote(Guid spotQuoteId)
    {
        return await spotQuoteRepository.ReadByIdAsync(spotQuoteId);
    }

    public async Task UpdateSpotQuotePrice(Guid spotQuoteId, double price)
    {
        var spotQuote = await spotQuoteRepository.ReadByIdAsync(spotQuoteId);
        spotQuote.Price = price;
        await spotQuoteRepository.UpdateAsync(spotQuote);
    }

    public async Task DeleteSpotQuote(Guid spotQuoteId)
    {
        var spotQuote = await spotQuoteRepository.ReadByIdAsync(spotQuoteId);
        await spotQuoteRepository.DeleteAsync(spotQuote);
    }

    /* Part 5 */
    public async Task<IReadOnlyCollection<Guid>> GetAllSpotQuoteIdsForCustomer(Guid customerId)
    {
        return await spotQuoteRepository.ReadAllProjectionsByFilterAsync(
            spotQuote => spotQuote.Id,
            spotQuote => spotQuote.CustomerId == customerId
            );
    }

    /* Part 6 */
    public async Task<IReadOnlyCollection<SpotQuote>> FetchExternalSpotQuotes()
    {
        return await apiGateway.GetAsync<IReadOnlyCollection<SpotQuote>>("/spotquotes");
    }

    /* Part 7 */
    public async Task<IReadOnlyCollection<SpotQuote>> GetSpotQuoteHistory(Guid spotQuoteId)
    {
        return await entityHistoryService.GetEntityHistoryAsync<SpotQuote>(spotQuoteId);
    }

    public async Task DebugTo(DateTime pointInTime)
    {
        await replayService.ReplayUntilAsync(pointInTime,
              useSnapshot: true,
              autoStop: false
            );
    }

    public async Task ResetEntityStore()
    {
        await replayService.ReplayAllAsync(
      useSnapshot: false,
      autoStop: true
    );
    }

    /* Part 10 */
    public async Task<int> NrOfEventsTheLastHour()
    {
        return (await eventStore.GetEventsFromAsync(
            DateTime.UtcNow.AddHours(-1)
            )).Count;
    }
}