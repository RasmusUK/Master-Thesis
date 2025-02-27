namespace EventSource.Core.Test;

public class TestAggregateRoot
{
    private readonly string from;
    private readonly string to;

    public TestAggregateRoot(string from, string to)
    {
        this.from = from;
        this.to = to;
    }
}