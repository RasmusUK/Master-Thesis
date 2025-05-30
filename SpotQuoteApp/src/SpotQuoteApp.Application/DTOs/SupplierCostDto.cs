using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Application.DTOs;

public class SupplierCostDto
{
    public ChargeType ChargeType { get; set; }
    public CostType CostType { get; set; }
    public double Value { get; set; }
    public double Weight { get; set; }
    public double Cbm { get; set; }

    public double Total { get; set; }
}
