using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.ValueObjects;

namespace SpotQuoteApp.Application.Mappers;

public static class ShippingDetailsMapper
{
    public static ShippingDetailsDto ToDto(this ShippingDetails shippingDetails)
    {
        return new ShippingDetailsDto
        {
            Collis = shippingDetails.Collis.Select(ColliMapper.ToDto).ToList(),
            Description = shippingDetails.Description,
            References = shippingDetails.References,
            ReadyToLoadDate = shippingDetails.ReadyToLoadDate,
            BookingProperties = shippingDetails.BookingProperties.ToList(),
        };
    }

    public static ShippingDetails ToDomain(this ShippingDetailsDto shippingDetailsDto)
    {
        return new ShippingDetails(
            shippingDetailsDto.Description,
            shippingDetailsDto.References,
            shippingDetailsDto.ReadyToLoadDate ?? DateTime.UtcNow
        )
        {
            Collis = shippingDetailsDto.Collis.Select(ColliMapper.ToDomain).ToList(),
            BookingProperties = shippingDetailsDto.BookingProperties.ToList(),
        };
    }
}
