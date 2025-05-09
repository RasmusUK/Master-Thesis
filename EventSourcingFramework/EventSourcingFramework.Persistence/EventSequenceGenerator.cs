using EventSourcingFramework.Persistence.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventSourcingFramework.Persistence;

public class EventSequenceGenerator : IEventSequenceGenerator
{
    private readonly IMongoCollection<BsonDocument> counters;

    public EventSequenceGenerator(IMongoDbService service)
    {
        counters = service.CounterCollection;
    }

    public async Task<long> GetNextSequenceNumberAsync()
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", "eventNumber");
        var update = Builders<BsonDocument>.Update.Inc("seq", 1);

        var options = new FindOneAndUpdateOptions<BsonDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After,
        };

        var result = await counters.FindOneAndUpdateAsync(filter, update, options);
        return result["seq"].ToInt64();
    }

    public async Task<long> GetCurrentSequenceNumberAsync()
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", "eventNumber");
        var result = await counters.Find(filter).FirstOrDefaultAsync();

        if (result == null || !result.Contains("seq"))
            return 0;

        return result["seq"].ToInt64();
    }
}
