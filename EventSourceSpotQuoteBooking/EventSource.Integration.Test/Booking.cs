namespace EventSource.Core.Test;

public class Booking : AggregateRoot
{
    private readonly Address from;
    private readonly Address to;

    public override void Apply(Event e)
    {
        throw new NotImplementedException();
    }
}
