using SpotQuoteBooking.EventSource.Core.ValueObjects.Enums;

namespace SpotQuoteBooking.EventSource.Core.ValueObjects;

public record SellingCost : BaseCost
{
    public double? MinimumValue { get; }
    public double? MaximumValue { get; }
    public double? Profit { get; }
    public new double Total { get; }

    public string Comment { get; }

    public SellingCost(
        ChargeType chargeType,
        CostType costType,
        double value,
        double weight,
        double cbm,
        string comment,
        double? minimumValue = null,
        double? maximumValue = null,
        double? profit = null
    )
        : base(chargeType, costType, value, weight, cbm)
    {
        Comment = comment;
        MinimumValue = minimumValue;
        MaximumValue = maximumValue;
        Profit = profit;
        Total = CalculateTotal(value, weight, cbm, minimumValue, maximumValue, profit, costType);
    }

    public static double CalculateTotal(
        double value,
        double weight,
        double cbm,
        double? minimumValue,
        double? maximumValue,
        double? profit,
        CostType costType
    )
    {
        var calculatedProfit = 0.0;

        if (profit is not null)
        {
            if (costType == CostType.PerKg)
                calculatedProfit = profit.Value * weight;
            else if (costType == CostType.PerCbm)
                calculatedProfit = profit.Value * cbm;
            else if (costType == CostType.PerShipment)
                calculatedProfit = profit.Value;
        }

        var min = minimumValue.GetValueOrDefault() == 0 ? null : minimumValue;
        var max = maximumValue.GetValueOrDefault() == 0 ? null : maximumValue;
        var total = BaseCost.CalculateTotal(value, weight, cbm, costType) + calculatedProfit;

        if (min is null && max is null)
            return total;

        if (min is null)
            return Math.Min(max!.Value, total);

        if (max is null)
            return Math.Max(min.Value, total);

        return Math.Max(min.Value, Math.Min(max.Value, total));
    }
}
