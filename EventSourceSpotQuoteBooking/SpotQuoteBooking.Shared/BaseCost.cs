namespace SpotQuoteBooking.Shared;

public class BaseCost
{
    public ChargeType ChargeType { get; set; }
    public CostType CostType { get; set; }
    public double Value { get; set; }
    public double Weight { get; set; }
    public double Cbm { get; set; }

    protected BaseCost(double weight, double cbm)
    {
        Weight = weight;
        Cbm = cbm;
    }

    protected double TotalValue
    {
        get
        {
            if (CostType == CostType.PerKg)
                return Value * Weight;
            if (CostType == CostType.PerCbm)
                return Value * Cbm;
            if (CostType == CostType.PerShipment)
                return Value;

            return 0;
        }
    }
}
