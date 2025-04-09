namespace SpotQuoteBooking.Shared;

public class SellingCost : BaseCost
{
    public double? MinimumValue { get; set; }
    public double? MaximumValue { get; set; }
    public double? Profit { get; set; }
    public new double TotalValue
    {
        get
        {
            var calculatedProfit = 0.0;

            if (Profit is not null)
            {
                if (CostType == CostType.PerKg)
                    calculatedProfit = Profit.Value * Weight;
                else if (CostType == CostType.PerCbm)
                    calculatedProfit = Profit.Value * Cbm;
                else if (CostType == CostType.PerShipment)
                    calculatedProfit = Profit.Value;
            }

            var min = MinimumValue.GetValueOrDefault() == 0 ? null : MinimumValue;
            var max = MaximumValue.GetValueOrDefault() == 0 ? null : MaximumValue;
            var value = base.TotalValue + calculatedProfit;

            if (min is null && max is null)
                return value;

            if (min is null)
                return Math.Min(max!.Value, value);

            if (max is null)
                return Math.Max(min.Value, value);

            return Math.Max(min.Value, Math.Min(max.Value, value));
        }
    }

    public string Comment { get; set; } = string.Empty;

    public SellingCost(double weight, double cbm)
        : base(weight, cbm) { }
}
