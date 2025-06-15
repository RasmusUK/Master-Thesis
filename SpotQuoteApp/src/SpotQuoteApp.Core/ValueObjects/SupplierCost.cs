using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Core.ValueObjects;

public record SupplierCost : BaseCost
{
    public SupplierCost(
        ChargeType chargeType,
        CostType costType,
        double value,
        double weight,
        double cbm
    )
        : base(chargeType, costType, value, weight, cbm) { }

    public static new double CalculateTotal(
        double value,
        double weight,
        double cbm,
        CostType costType
    ) => BaseCost.CalculateTotal(value, weight, cbm, costType);
}
