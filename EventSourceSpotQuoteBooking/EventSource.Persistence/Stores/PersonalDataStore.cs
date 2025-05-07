using EventSource.Core.Interfaces;
using EventSource.Core.Options;
using EventSource.Persistence.Interfaces;
using EventSource.Persistence.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EventSource.Persistence.Stores;

public class PersonalDataStore : IPersonalDataStore
{
    private readonly IMongoCollection<PersonalData> collection;
    private readonly EventSourcingOptions eventSourcingOptions;

    public PersonalDataStore(
        IMongoDbService mongoDbService,
        IOptionsMonitor<EventSourcingOptions> eventSourcingOptions
    )
    {
        collection = mongoDbService.PersonalDataCollection;
        this.eventSourcingOptions = eventSourcingOptions.CurrentValue;
    }

    public async Task StoreAsync(Guid eventId, Dictionary<string, object?> data)
    {
        if (!eventSourcingOptions.EnablePersonalDataStore)
            return;

        var record = new PersonalData { EventId = eventId, Data = data };

        await collection.ReplaceOneAsync(
            Builders<PersonalData>.Filter.Eq(r => r.EventId, eventId),
            record,
            new ReplaceOptions { IsUpsert = true }
        );
    }

    public async Task<Dictionary<string, object?>> RetrieveAsync(Guid eventId)
    {
        if (!eventSourcingOptions.EnablePersonalDataStore)
            return new Dictionary<string, object?>();

        var record = await collection
            .Find(Builders<PersonalData>.Filter.Eq(r => r.EventId, eventId))
            .FirstOrDefaultAsync();

        return record?.Data ?? new Dictionary<string, object?>();
    }
}
