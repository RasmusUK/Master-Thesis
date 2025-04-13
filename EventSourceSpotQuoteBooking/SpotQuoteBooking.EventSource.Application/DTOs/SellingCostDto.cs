using SpotQuoteBooking.EventSource.Core.ValueObjects.Enums;

namespace SpotQuoteBooking.EventSource.Application.DTOs;

public class SellingCostDto
{
    public ChargeType ChargeType { get; set; }
    public CostType CostType { get; set; }
    public double Value { get; set; }
    public double Weight { get; set; }
    public double Cbm { get; set; }
    public double? MinimumValue { get; set; }
    public double? MaximumValue { get; set; }
    public double? Profit { get; set; }
    public string Comment { get; set; }
    public double Total { get; set; }
}
