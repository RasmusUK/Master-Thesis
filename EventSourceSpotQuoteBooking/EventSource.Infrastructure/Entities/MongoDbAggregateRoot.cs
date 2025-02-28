using EventSource.Core;
using Newtonsoft.Json;

namespace EventSource.Persistence.Entities;

public class MongoDbAggregateRoot : MongoDbEntity<AggregateRoot>
{
    public MongoDbAggregateRoot(AggregateRoot aggregateRoot)
        : base(aggregateRoot.Id, aggregateRoot) { }
}
