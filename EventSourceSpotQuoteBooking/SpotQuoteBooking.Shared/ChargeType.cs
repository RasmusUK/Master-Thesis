namespace SpotQuoteBooking.Shared;

public record ChargeType(string Value) : IComparable
{
    public static readonly ChargeType Freight = new("Freight");
    public static readonly ChargeType FuelSurcharge = new("Fuel Surcharge");
    public static readonly ChargeType CustomsClearance = new("Customs Clearance");
    public static readonly ChargeType DutiesAndTaxes = new("Duties and Taxes");
    public static readonly ChargeType Insurance = new("Insurance");
    public static readonly ChargeType Documentation = new("Documentation");
    public static readonly ChargeType PortHandling = new("Port/Terminal Handling");
    public static readonly ChargeType Inspection = new("Inspection");
    public static readonly ChargeType Demurrage = new("Demurrage");
    public static readonly ChargeType Detention = new("Detention");
    public static readonly ChargeType Storage = new("Storage");
    public static readonly ChargeType Delivery = new("Door Delivery");
    public static readonly ChargeType BondFee = new("Bond Fee");
    public static readonly ChargeType ReeferSurcharge = new("Reefer Surcharge");
    public static readonly ChargeType DangerousGoods = new("Dangerous Goods");
    public static readonly ChargeType BunkerAdjustment = new("Bunker Adjustment (BAF)");
    public static readonly ChargeType CurrencyAdjustment = new("Currency Adjustment (CAF)");

    public override string ToString() => Value;

    public int CompareTo(object? obj)
    {
        if (obj is ChargeType other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(ChargeType)}");
    }

    public static IReadOnlyCollection<ChargeType> GetAll() =>
        new List<ChargeType>
        {
            Freight,
            FuelSurcharge,
            CustomsClearance,
            DutiesAndTaxes,
            Insurance,
            Documentation,
            PortHandling,
            Inspection,
            Demurrage,
            Detention,
            Storage,
            Delivery,
            BondFee,
            ReeferSurcharge,
            DangerousGoods,
        }
            .OrderBy(x => x.Value)
            .ToList();

    public static IReadOnlyCollection<ChargeType> GetByTransportMode(TransportMode mode)
    {
        var charges = new List<ChargeType>();

        if (mode == TransportMode.Air)
            charges = new List<ChargeType>
            {
                Freight,
                FuelSurcharge,
                CustomsClearance,
                Documentation,
                Insurance,
                DangerousGoods,
            };

        if (mode == TransportMode.Sea)
            charges = new List<ChargeType>
            {
                Freight,
                BunkerAdjustment,
                CurrencyAdjustment,
                PortHandling,
                CustomsClearance,
                Demurrage,
                Detention,
                Insurance,
                Documentation,
            };

        if (mode == TransportMode.Road)
            charges = new List<ChargeType>
            {
                Freight,
                FuelSurcharge,
                Delivery,
                CustomsClearance,
                Insurance,
                Documentation,
            };

        if (mode == TransportMode.Courier)
            charges = new List<ChargeType>
            {
                Freight,
                FuelSurcharge,
                Delivery,
                CustomsClearance,
                Insurance,
            };

        return charges.OrderBy(x => x.Value).ToList();
    }
}
