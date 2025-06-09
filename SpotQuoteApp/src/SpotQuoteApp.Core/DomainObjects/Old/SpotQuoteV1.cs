using EventSourcingFramework.Core.Models.Entity;
using SpotQuoteApp.Core.ValueObjects;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Core.DomainObjects.Old;

public class SpotQuoteV1 : Entity
{
    public SpotQuoteV1(
        Guid addressFromId,
        Guid addressToId,
        Direction direction,
        TransportMode transportMode,
        Incoterm incoterm,
        ShippingDetails shippingDetails,
        DateTime validUntil,
        Customer customer,
        MailOptions mailOptions,
        string internalComments,
        ICollection<Quote> quotes
    )
    {
        AddressFromId = addressFromId;
        AddressToId = addressToId;
        Direction = direction;
        TransportMode = transportMode;
        Incoterm = incoterm;
        ShippingDetails = shippingDetails;
        ValidUntil = validUntil;
        Customer = customer;
        MailOptions = mailOptions;
        InternalComments = internalComments;
        Quotes = quotes;
        SchemaVersion = 1;
    }

    public Guid AddressFromId { get; set; }
    public Guid AddressToId { get; set; }
    public Direction Direction { get; set; }
    public TransportMode TransportMode { get; set; }
    public Incoterm Incoterm { get; set; }
    public ShippingDetails ShippingDetails { get; set; }
    public DateTime ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Customer Customer { get; set; }
    public ICollection<Quote> Quotes { get; set; }
    public MailOptions MailOptions { get; set; }
    public string InternalComments { get; set; }
    public double TotalWeight => ShippingDetails.Collis.Sum(c => c.Weight);

    public double TotalCbm => ShippingDetails.Collis.Sum(c => c.Cbm);
}
