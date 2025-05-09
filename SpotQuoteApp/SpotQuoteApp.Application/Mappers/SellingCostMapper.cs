using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.ValueObjects;

namespace SpotQuoteApp.Application.Mappers;

public static class SellingCostMapper
{
    public static SellingCostDto ToDto(this SellingCost sellingCost)
    {
        return new SellingCostDto
        {
            Value = sellingCost.Value,
            ChargeType = sellingCost.ChargeType,
            CostType = sellingCost.CostType,
            Weight = sellingCost.Weight,
            Cbm = sellingCost.Cbm,
            Total = sellingCost.Total,
            MinimumValue = sellingCost.MinimumValue,
            MaximumValue = sellingCost.MaximumValue,
            Profit = sellingCost.Profit,
            Comment = sellingCost.Comment,
        };
    }

    public static SellingCost ToDomain(this SellingCostDto sellingCostDto)
    {
        return new SellingCost(
            sellingCostDto.ChargeType,
            sellingCostDto.CostType,
            sellingCostDto.Value,
            sellingCostDto.Weight,
            sellingCostDto.Cbm,
            sellingCostDto.Comment,
            sellingCostDto.MinimumValue,
            sellingCostDto.MaximumValue,
            sellingCostDto.Profit
        );
    }
}
