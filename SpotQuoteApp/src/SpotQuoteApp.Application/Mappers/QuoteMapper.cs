using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.ValueObjects;

namespace SpotQuoteApp.Application.Mappers;

public static class QuoteMapper
{
    public static QuoteDto ToDto(this Quote quote)
    {
        return new QuoteDto
        {
            Supplier = quote.Supplier,
            ForwarderService = quote.ForwarderService,
            SupplierService = quote.SupplierService,
            Profit = quote.Profit.ToDto(),
            IsAllIn = quote.IsAllIn,
            Costs = quote.Costs.Select(c => c.ToDto()).ToList(),
            CommentsExternal = quote.CommentsExternal,
            CommentsInternal = quote.CommentsInternal,
            TotalPrice = quote.TotalPrice,
            TotalProfit = quote.TotalProfit,
            Status = quote.Status,
        };
    }

    public static Quote ToDomain(this QuoteDto quoteDto)
    {
        return new Quote(
            quoteDto.Supplier,
            quoteDto.ForwarderService,
            quoteDto.SupplierService,
            quoteDto.Profit.ToDomain(),
            quoteDto.IsAllIn,
            quoteDto.Costs.Select(c => c.ToDomain()).ToList(),
            quoteDto.CommentsExternal,
            quoteDto.CommentsInternal,
            quoteDto.Status
        );
    }
}
