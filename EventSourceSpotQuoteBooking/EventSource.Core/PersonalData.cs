namespace EventSource.Core;

public class PersonalData
{
    public Guid EventId { get; init; }
    public object Data { get; init; }
    public string Name { get; init; }

    public PersonalData(Guid eventId, object obj, string name)
    {
        EventId = eventId;
        Data = obj;
        Name = name;
    }
}
