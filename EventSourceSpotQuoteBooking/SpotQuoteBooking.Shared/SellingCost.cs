namespace SpotQuoteBooking.Shared;

public class SellingCost : BaseCost
{
    public double MinimumValue { get; set; }
    public double MaximumValue { get; set; }
    public double Profit { get; set; }
    public new double TotalValue
    {
        get
        {
            if (CostType == CostType.PerKg)
                return Math.Max(
                    MinimumValue,
                    Math.Min(MaximumValue, base.TotalValue + Profit * Weight)
                );
            if (CostType == CostType.PerCbm)
                return Math.Max(
                    MinimumValue,
                    Math.Min(MaximumValue, base.TotalValue + Profit * Cbm)
                );
            if (CostType == CostType.PerShipment)
                return Math.Max(MinimumValue, Math.Min(MaximumValue, base.TotalValue + Profit));

            return 0;
        }
    }

    public string Comment { get; set; } = string.Empty;

    public SellingCost(double weight, double cbm)
        : base(weight, cbm) { }
}
