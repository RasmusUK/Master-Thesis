namespace SpotQuoteBooking.Shared;

public class ShippingDetails
{
    public ICollection<Colli> Collis { get; set; }
    public string Description { get; set; }
    public string References { get; set; }
    public DateTime? ReadyToLoadDate { get; set; }
    public ICollection<BookingProperty> BookingProperties { get; set; } =
        new List<BookingProperty>();

    public ShippingDetails() { }

    public ShippingDetails(
        ICollection<Colli> Collis,
        string Description,
        DateTime? ReadyToLoadDate,
        string references
    )
    {
        this.Collis = Collis;
        this.Description = Description;
        this.ReadyToLoadDate = ReadyToLoadDate;
        References = references;
    }

    public ShippingDetails(
        ICollection<Colli> Collis,
        string Description,
        DateTime? ReadyToLoadDate,
        string references,
        ICollection<BookingProperty> bookingProperties
    )
    {
        this.Collis = Collis;
        this.Description = Description;
        this.ReadyToLoadDate = ReadyToLoadDate;
        References = references;
        BookingProperties = bookingProperties;
    }
}
