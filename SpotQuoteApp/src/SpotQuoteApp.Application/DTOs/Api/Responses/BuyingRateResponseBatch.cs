namespace SpotQuoteApp.Application.DTOs.Api.Responses;

public class BuyingRateResponseBatch
{
    public List<BuyingRateResponse> Rates { get; set; } = new();
}