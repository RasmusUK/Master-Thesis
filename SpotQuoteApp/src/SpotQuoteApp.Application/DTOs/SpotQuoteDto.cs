using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Application.DTOs;

public class SpotQuoteDto
{
    public Guid Id { get; set; }
    public int ConcurrencyVersion { get; set; }
    public AddressDto AddressFrom { get; set; }
    public AddressDto AddressTo { get; set; }
    public Direction Direction { get; set; }
    public Incoterm Incoterm { get; set; }
    public TransportMode TransportMode { get; set; }
    public ShippingDetailsDto ShippingDetails { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public CustomerDto Customer { get; set; }
    public ICollection<QuoteDto> Quotes { get; set; } = new List<QuoteDto>();
    public MailOptionsDto MailOptions { get; set; }
    public string InternalComments { get; set; }
    public double TotalWeight { get; set; }
    public double TotalCbm { get; set; }
    public bool IsDraft() => Quotes.Any(q => q.Status == BookingStatus.Draft);
}
