using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.ValueObjects;

namespace SpotQuoteApp.Application.Mappers;

public static class ProfitMapper
{
    public static ProfitDto ToDto(this Profit profit)
    {
        return new ProfitDto { Value = profit.Value, IsPercentage = profit.IsPercentage };
    }

    public static Profit ToDomain(this ProfitDto profitDto)
    {
        return new Profit(profitDto.Value, profitDto.IsPercentage);
    }
}
