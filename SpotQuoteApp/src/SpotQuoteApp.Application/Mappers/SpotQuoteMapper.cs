using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.AggregateRoots;

namespace SpotQuoteApp.Application.Mappers;

public static class SpotQuoteMapper
{
    public static SpotQuoteDto ToDto(
        this SpotQuote spotQuote,
        CustomerDto customer,
        AddressDto addressFrom,
        AddressDto addressTo,
        ICollection<UserDto> userRecipients
    )
    {
        return new SpotQuoteDto
        {
            Id = spotQuote.Id,
            Customer = customer,
            AddressFrom = addressFrom,
            AddressTo = addressTo,
            CreatedAt = spotQuote.CreatedAt,
            Direction = spotQuote.Direction,
            Incoterm = spotQuote.Incoterm,
            TransportMode = spotQuote.TransportMode,
            ShippingDetails = spotQuote.ShippingDetails.ToDto(),
            ValidUntil = spotQuote.ValidUntil,
            Quotes = spotQuote.Quotes.Select(QuoteMapper.ToDto).ToList(),
            MailOptions = spotQuote.MailOptions.ToDto(userRecipients),
            InternalComments = spotQuote.InternalComments,
            TotalWeight = spotQuote.TotalWeight,
            TotalCbm = spotQuote.TotalCbm,
            ConcurrencyVersion = spotQuote.ConcurrencyVersion,
        };
    }

    public static SpotQuote ToDomain(this SpotQuoteDto spotQuoteDto)
    {
        return new SpotQuote(
            spotQuoteDto.AddressFrom.Id,
            spotQuoteDto.AddressTo.Id,
            spotQuoteDto.Direction,
            spotQuoteDto.TransportMode,
            spotQuoteDto.Incoterm,
            spotQuoteDto.ShippingDetails.ToDomain(),
            spotQuoteDto.ValidUntil.Value,
            spotQuoteDto.Customer.Id,
            spotQuoteDto.MailOptions.ToDomain(),
            spotQuoteDto.InternalComments,
            spotQuoteDto.Quotes.Select(QuoteMapper.ToDomain).ToList()
        )
        {
            Id = spotQuoteDto.Id,
            ConcurrencyVersion = spotQuoteDto.ConcurrencyVersion,
        };
    }
}
