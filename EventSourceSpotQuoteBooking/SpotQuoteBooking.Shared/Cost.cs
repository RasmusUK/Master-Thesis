namespace SpotQuoteBooking.Shared;

public class Cost
{
    public SupplierCost SupplierCost { get; set; }
    public SellingCost SellingCost { get; set; }

    public Cost(double weight, double cbm)
    {
        SupplierCost = new SupplierCost(weight, cbm);
        SellingCost = new SellingCost(weight, cbm);
    }
}
