namespace EventSource.Core.Test;

public class Booking : IEntity
{
    public string CustomerName { get; set; }
    public Address From { get; set; }
    public Address To { get; set; }
    public Address Test { get; set; }
    public string? Name { get; set; }

    public Guid Id { get; } = Guid.NewGuid();
}
