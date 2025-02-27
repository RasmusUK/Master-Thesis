namespace EventSource.Core.Test;

public class TestEvent : Event
{
    public string From { get; private set; }
    public string To { get; private set; }
    public Address Address { get; private set; }
    
    public TestEvent(string from, string to, Address address)
    {
        this.From = from;
        this.To = to;
        Address = address;
    }
}