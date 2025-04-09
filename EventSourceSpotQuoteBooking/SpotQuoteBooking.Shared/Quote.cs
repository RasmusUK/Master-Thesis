namespace SpotQuoteBooking.Shared;

public class Quote
{
    public Supplier? Supplier { get; set; }
    public ForwarderService? ForwarderService { get; set; }
    public SupplierService? SupplierService { get; set; }
    public Profit Profit { get; set; } = new();
    public bool IsAllIn { get; set; }
    public ICollection<Cost> Costs { get; set; } = new List<Cost>();

    public double TotalPrice
    {
        get
        {
            var sellingValue = Costs.Sum(c => c.SellingCost.TotalValue);
            var profit = Profit.CalculatedProfit(sellingValue);
            return sellingValue + profit;
        }
    }

    public double TotalProfit
    {
        get
        {
            var sellingValue = Costs.Sum(c => c.SellingCost.TotalValue);
            var supplierValue = Costs.Sum(c => c.SupplierCost.CalculatedValue);
            return sellingValue - supplierValue + Profit.CalculatedProfit(sellingValue);
        }
    }
}
