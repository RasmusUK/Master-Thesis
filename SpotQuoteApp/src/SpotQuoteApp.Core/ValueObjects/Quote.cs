using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Core.ValueObjects;

public record Quote
{
    public Quote(
        Supplier supplier,
        ForwarderService forwarderService,
        SupplierService supplierService,
        Profit profit,
        bool isAllIn,
        ICollection<Cost> costs,
        string commentsExternal,
        string commentsInternal
    )
    {
        Supplier = supplier;
        ForwarderService = forwarderService;
        SupplierService = supplierService;
        Profit = profit;
        IsAllIn = isAllIn;
        Costs = costs;
        CommentsExternal = commentsExternal;
        CommentsInternal = commentsInternal;

        (TotalPrice, TotalProfit) = CalculateTotal(costs, profit);
    }

    public Supplier Supplier { get; }
    public ForwarderService ForwarderService { get; }
    public SupplierService SupplierService { get; }
    public Profit Profit { get; }
    public bool IsAllIn { get; }
    public ICollection<Cost> Costs { get; }
    public string CommentsExternal { get; }
    public string CommentsInternal { get; }

    public double TotalPrice { get; }
    public double TotalProfit { get; }

    public static (double totalPrice, double totalProfit) CalculateTotal(
        ICollection<Cost> costs,
        Profit profit
    )
    {
        var sellingValue = costs.Sum(c => c.SellingCost.Total);
        var supplierValue = costs.Sum(c => c.SupplierCost.Total);
        var profitValue = profit.CalculatedProfit(sellingValue);
        var totalPrice = sellingValue + profitValue;
        var totalProfit = sellingValue - supplierValue + profitValue;

        return (totalPrice, totalProfit);
    }
}
