using SpotQuoteBooking.EventSource.Application.DTOs;
using SpotQuoteBooking.EventSource.Core.ValueObjects;

namespace SpotQuoteBooking.EventSource.Application.Mappers;

public static class SupplierCostMapper
{
    public static SupplierCostDto ToDto(this SupplierCost supplierCost)
    {
        return new SupplierCostDto
        {
            Value = supplierCost.Value,
            ChargeType = supplierCost.ChargeType,
            CostType = supplierCost.CostType,
            Weight = supplierCost.Weight,
            Cbm = supplierCost.Cbm,
            Total = supplierCost.Total,
        };
    }

    public static SupplierCost ToDomain(this SupplierCostDto supplierCostDto)
    {
        return new SupplierCost(
            supplierCostDto.ChargeType,
            supplierCostDto.CostType,
            supplierCostDto.Value,
            supplierCostDto.Weight,
            supplierCostDto.Cbm
        );
    }
}
