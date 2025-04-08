namespace SpotQuoteBooking.Shared;

public class Quote
{
    public ICollection<Cost> Costs { get; set; } = new List<Cost>();
}
