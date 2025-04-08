namespace SpotQuoteBooking.Shared;

public record ColliType(string Value) : IComparable
{
    public static readonly ColliType Pallet = new("Pallet");
    public static readonly ColliType Box = new("Box");
    public static readonly ColliType Container = new("Container");
    public static readonly ColliType Envelope = new("Envelope");
    public static readonly ColliType Crate = new("Crate");
    public static readonly ColliType Drum = new("Drum");
    public static readonly ColliType Roll = new("Roll");
    public static readonly ColliType Bag = new("Bag");
    public static readonly ColliType Bale = new("Bale");
    public static readonly ColliType Barrel = new("Barrel");
    public static readonly ColliType Bundle = new("Bundle");
    public static readonly ColliType Cage = new("Cage");
    public static readonly ColliType Canister = new("Canister");
    public static readonly ColliType Case = new("Case");
    public static readonly ColliType Cask = new("Cask");
    public static readonly ColliType Reels = new("Reels");
    public static readonly ColliType Sack = new("Sack");
    public static readonly ColliType Tub = new("Tub");
    public static readonly ColliType Tank = new("Tank");
    public static readonly ColliType Tray = new("Tray");

    public override string ToString() => Value;

    public int CompareTo(object? obj)
    {
        if (obj is ColliType other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(ColliType)}");
    }

    public static IReadOnlyCollection<ColliType> GetAll() =>
        new List<ColliType>
        {
            Pallet,
            Box,
            Container,
            Envelope,
            Crate,
            Drum,
            Roll,
            Bag,
            Bale,
            Barrel,
            Bundle,
            Cage,
            Canister,
            Case,
            Cask,
            Reels,
            Sack,
            Tub,
            Tank,
            Tray,
        }
            .OrderBy(x => x.Value)
            .ToList();

    public static IReadOnlyCollection<ColliType> GetByTransportMode(TransportMode mode)
    {
        var colliTypes = new List<ColliType>();
        if (mode == TransportMode.Air)
            colliTypes = new List<ColliType> { Box, Pallet, Envelope, Crate, Bag, Case };
        if (mode == TransportMode.Sea)
            colliTypes = new List<ColliType>
            {
                Container,
                Pallet,
                Crate,
                Drum,
                Tank,
                Barrel,
                Bale,
                Sack,
            };
        if (mode == TransportMode.Road)
            colliTypes = new List<ColliType> { Pallet, Box, Crate, Drum, Roll, Bag, Tray, Bundle };
        if (mode == TransportMode.Courier)
            colliTypes = new List<ColliType> { Envelope, Box, Bag, Case };

        return colliTypes.OrderBy(x => x.Value).ToList();
    }
}
