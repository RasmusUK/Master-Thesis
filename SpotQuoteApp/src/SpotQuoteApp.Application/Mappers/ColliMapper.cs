using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.ValueObjects;

namespace SpotQuoteApp.Application.Mappers;

public static class ColliMapper
{
    public static ColliDto ToDto(this Colli colli)
    {
        return new ColliDto
        {
            Weight = colli.Weight,
            Cbm = colli.Cbm,
            Length = colli.Length,
            Width = colli.Width,
            Height = colli.Height,
            NumberOfUnits = colli.NumberOfUnits,
            Type = colli.Type,
        };
    }

    public static Colli ToDomain(this ColliDto colliDto)
    {
        return new Colli(
            colliDto.NumberOfUnits,
            colliDto.Type,
            colliDto.Length,
            colliDto.Width,
            colliDto.Height,
            colliDto.Weight
        );
    }
}
