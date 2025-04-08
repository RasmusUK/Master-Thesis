namespace SpotQuoteBooking.Shared;

public class ShippingDetails
{
    public ICollection<Colli> Collis { get; set; }
    public string Description { get; set; }
    public string References { get; set; }
    public DateTime? ReadyToLoadDate { get; set; }

    public ShippingDetails(string references)
    {
        References = references;
    }

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
}
