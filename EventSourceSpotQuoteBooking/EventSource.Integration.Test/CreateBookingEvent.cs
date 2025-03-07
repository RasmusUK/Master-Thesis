namespace EventSource.Core.Test;

public record CreateBookingEvent(Address From, Address To) : Event;
