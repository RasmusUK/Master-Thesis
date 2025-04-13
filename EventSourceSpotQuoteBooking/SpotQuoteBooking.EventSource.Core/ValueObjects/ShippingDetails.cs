using SpotQuoteBooking.EventSource.Core.ValueObjects.Enums;

namespace SpotQuoteBooking.EventSource.Core.ValueObjects;

public class ShippingDetails
{
    public ShippingDetails(string description, string references, DateTime readyToLoadDate)
    {
        Description = description;
        References = references;
        ReadyToLoadDate = readyToLoadDate;
    }

    public ICollection<Colli> Collis { get; set; } = new List<Colli>();
    public string Description { get; set; }
    public string References { get; set; }
    public DateTime ReadyToLoadDate { get; set; }
    public ICollection<BookingProperty> BookingProperties { get; set; } =
        new List<BookingProperty>();
}
