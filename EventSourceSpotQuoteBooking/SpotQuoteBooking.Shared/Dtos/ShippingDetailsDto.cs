namespace SpotQuoteBooking.Shared.Dtos;

public class ShippingDetailsDto
{
    public ColliDto[] Collis { get; set; }
    public string Description { get; set; }
    public DateTime ReadyToLoadDate { get; set; }

    public ShippingDetails ToShippingDetails()
    {
        return new ShippingDetails(
            Collis.Select(c => c.ToColli()).ToList(),
            Description,
            ReadyToLoadDate
        );
    }
}
