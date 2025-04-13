using System.Text.Json;

namespace EventSource.Core.Events;

public record RepoEvent<T>(T Entity) : Event(Entity.Id)
    where T : Entity
{
    public override string ContentString => JsonSerializer.Serialize(Entity, jsonSerializerOptions);

    private readonly JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };
}
