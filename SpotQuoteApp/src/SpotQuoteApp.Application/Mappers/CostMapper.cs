using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.ValueObjects;

namespace SpotQuoteApp.Application.Mappers;

public static class CostMapper
{
    public static CostDto ToDto(this Cost cost)
    {
        return new CostDto
        {
            SupplierCost = cost.SupplierCost.ToDto(),
            SellingCost = cost.SellingCost.ToDto(),
        };
    }

    public static Cost ToDomain(this CostDto costDto)
    {
        return new Cost(costDto.SupplierCost.ToDomain(), costDto.SellingCost.ToDomain());
    }
}
