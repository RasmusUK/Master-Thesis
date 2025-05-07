using EventSource.Core.Interfaces;
using EventSource.Persistence.Interfaces;
using MongoDB.Driver;

namespace EventSource.Persistence.Stores;

public class PersonalDataStore : IPersonalDataStore
{
    private readonly IMongoCollection<PersonalData> collection;

    public PersonalDataStore(IMongoDbService mongoDbService)
    {
        collection = mongoDbService.PersonalDataCollection;
    }

    public async Task StoreAsync(Guid eventId, Dictionary<string, object?> data)
    {
        var record = new PersonalData { EventId = eventId, Data = data };

        await collection.ReplaceOneAsync(
            Builders<PersonalData>.Filter.Eq(r => r.EventId, eventId),
            record,
            new ReplaceOptions { IsUpsert = true }
        );
    }

    public async Task<Dictionary<string, object?>> RetrieveAsync(Guid eventId)
    {
        var record = await collection
            .Find(Builders<PersonalData>.Filter.Eq(r => r.EventId, eventId))
            .FirstOrDefaultAsync();

        return record?.Data ?? new Dictionary<string, object?>();
    }
}
