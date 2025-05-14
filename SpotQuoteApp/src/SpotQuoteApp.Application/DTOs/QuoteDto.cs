using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Application.DTOs;

public class QuoteDto
{
    public Supplier Supplier { get; set; }
    public ForwarderService ForwarderService { get; set; }
    public SupplierService SupplierService { get; set; }
    public ProfitDto Profit { get; set; }
    public bool IsAllIn { get; set; }
    public ICollection<CostDto> Costs { get; set; } = new List<CostDto>();
    public string CommentsExternal { get; set; }
    public string CommentsInternal { get; set; }
    public double TotalPrice { get; set; }
    public double TotalProfit { get; set; }
}
