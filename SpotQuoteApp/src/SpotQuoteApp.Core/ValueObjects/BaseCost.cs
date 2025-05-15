using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Core.ValueObjects;

public record BaseCost
{
    protected BaseCost(
        ChargeType chargeType,
        CostType costType,
        double value,
        double weight,
        double cbm
    )
    {
        ChargeType = chargeType;
        CostType = costType;
        Value = value;
        Weight = weight;
        Cbm = cbm;
        Total = CalculateTotal(value, weight, cbm, costType);
    }

    public ChargeType ChargeType { get; }
    public CostType CostType { get; }
    public double Value { get; }
    public double Weight { get; }
    public double Cbm { get; }

    public double Total { get; }

    protected static double CalculateTotal(
        double value,
        double weight,
        double cbm,
        CostType costType
    )
    {
        if (costType == CostType.PerKg)
            return value * weight;
        if (costType == CostType.PerCbm)
            return value * cbm;
        if (costType == CostType.PerShipment)
            return value;

        return 0;
    }
}
