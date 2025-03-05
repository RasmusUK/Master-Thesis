using EventSource.Core;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Entities;
using MongoDB.Driver;

namespace EventSource.Persistence.Stores;

public class MongoDbPersonalDataStore : IPersonalDataStore
{
    private readonly IMongoCollection<MongoDbPersonalData> collection;

    public MongoDbPersonalDataStore(IMongoCollection<MongoDbPersonalData> collection)
    {
        this.collection = collection;
    }

    public Task SavePersonalDataAsync(PersonalData p) =>
        collection.InsertOneAsync(new MongoDbPersonalData(p));

    public async Task<ICollection<PersonalData>> GetPersonalDataForEventAsync(Guid eventId)
    {
        var mongoPersonalInfos = await collection.Find(pi => pi.Id == eventId).ToListAsync();
        return mongoPersonalInfos.Select(e => e.ToDomain()).ToList();
    }
}
