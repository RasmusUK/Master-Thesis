using SpotQuoteBooking.EventSource.Core.ValueObjects.Enums;

namespace SpotQuoteBooking.EventSource.Application.DTOs;

public class ShippingDetailsDto
{
    public ICollection<ColliDto> Collis { get; set; } = new List<ColliDto>();
    public string Description { get; set; }
    public string References { get; set; }
    public DateTime? ReadyToLoadDate { get; set; }
    public ICollection<BookingProperty> BookingProperties { get; set; } =
        new List<BookingProperty>();
}
