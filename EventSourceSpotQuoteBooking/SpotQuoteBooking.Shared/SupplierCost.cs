namespace SpotQuoteBooking.Shared;

public class SupplierCost : BaseCost
{
    public double CalculatedValue
    {
        get => TotalValue;
    }

    public SupplierCost(double weight, double cbm)
        : base(weight, cbm) { }
}
