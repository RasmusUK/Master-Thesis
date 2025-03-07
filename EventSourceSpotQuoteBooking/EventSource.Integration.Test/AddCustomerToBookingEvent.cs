namespace EventSource.Core.Test;

public record AddCustomerToBookingEvent : Event
{
    [PersonalData]
    public string CustomerName { get; init; }

    public AddCustomerToBookingEvent() { }

    public AddCustomerToBookingEvent(Guid entityId, string customerName)
        : base(entityId)
    {
        CustomerName = customerName;
    }
}
