namespace SpotQuoteApp.Core.ValueObjects.Enums;

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
    
    public static ChargeType FromString(string value)
    {
        return value switch
        {
            "Freight" => Freight,
            "Fuel Surcharge" => FuelSurcharge,
            "Customs Clearance" => CustomsClearance,
            "Duties and Taxes" => DutiesAndTaxes,
            "Insurance" => Insurance,
            "Documentation" => Documentation,
            "Port/Terminal Handling" => PortHandling,
            "Inspection" => Inspection,
            "Demurrage" => Demurrage,
            "Detention" => Detention,
            "Storage" => Storage,
            "Door Delivery" => Delivery,
            "Bond Fee" => BondFee,
            "Reefer Surcharge" => ReeferSurcharge,
            "Dangerous Goods" => DangerousGoods,
            "Bunker Adjustment (BAF)" => BunkerAdjustment,
            "Currency Adjustment (CAF)" => CurrencyAdjustment,
            _ => throw new ArgumentException($"Invalid charge type: {value}")
        };
    }

    public int CompareTo(object? obj)
    {
        if (obj is ChargeType other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(ChargeType)}");
    }

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
                DutiesAndTaxes,
                Inspection,
                Storage,
                BondFee,
                ReeferSurcharge,
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
                DutiesAndTaxes,
                Inspection,
                Storage,
                BondFee,
                ReeferSurcharge,
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
                DutiesAndTaxes,
                Inspection,
                Storage,
                BondFee,
                ReeferSurcharge,
            };

        if (mode == TransportMode.Courier)
            charges = new List<ChargeType>
            {
                Freight,
                FuelSurcharge,
                Delivery,
                CustomsClearance,
                Insurance,
                DutiesAndTaxes,
                Inspection,
                Storage,
                BondFee,
                ReeferSurcharge,
            };

        return charges.OrderBy(x => x.Value).ToList();
    }
}
