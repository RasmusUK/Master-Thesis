namespace EventSource.Core;

public abstract record Event
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid? EntityId { get; set; }
    public Guid? TransactionId { get; init; }
    public string Typename
    {
        get
        {
            var type = GetType();

            if (!type.IsGenericType)
                return type.Name;

            var baseName = type.Name.Split('`')[0];
            var genericArgs = type.GetGenericArguments().Select(t => t.Name).ToArray();
            return $"{baseName}<{string.Join(", ", genericArgs)}>";
        }
    }

    public bool Compensation { get; init; } = false;

    public virtual string ContentString => string.Empty;

    protected Event() { }

    protected Event(Guid entityId)
    {
        EntityId = entityId;
    }
}
