using EventSourcing.Framework.Infrastructure.Shared.Configuration.Options;
using EventSourcing.Framework.Infrastructure.Shared.Interfaces;
using EventSourcing.Framework.Infrastructure.Shared.Models;
using EventSourcingFramework.Core.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EventSourcingFramework.Infrastructure.Stores.PersonalDataStore;

public class PersonalDataStore : IPersonalDataStore
{
    private readonly IMongoCollection<MongoPersonalData> collection;
    private readonly EventSourcingOptions eventSourcingOptions;

    public PersonalDataStore(
        IMongoDbService mongoDbService,
        IOptions<EventSourcingOptions> eventSourcingOptions
    )
    {
        collection = mongoDbService.PersonalDataCollection;
        this.eventSourcingOptions = eventSourcingOptions.Value;
    }

    public async Task StoreAsync(Guid eventId, Dictionary<string, object?> data)
    {
        if (!eventSourcingOptions.EnablePersonalDataStore)
            return;

        var record = new MongoPersonalData { EventId = eventId, Data = data };

        await collection.ReplaceOneAsync(
            Builders<MongoPersonalData>.Filter.Eq(r => r.EventId, eventId),
            record,
            new ReplaceOptions { IsUpsert = true }
        );
    }

    public async Task<Dictionary<string, object?>> RetrieveAsync(Guid eventId)
    {
        if (!eventSourcingOptions.EnablePersonalDataStore)
            return new Dictionary<string, object?>();

        var record = await collection
            .Find(Builders<MongoPersonalData>.Filter.Eq(r => r.EventId, eventId))
            .FirstOrDefaultAsync();

        return record?.Data ?? new Dictionary<string, object?>();
    }
}
