namespace SpotQuoteApp.Application.DTOs.Api.Requests;

public record BuyingRateRequest(string Supplier, string ForwarderService, string SupplierService, string CountryFrom,
    string CountryTo, string TransportMode);
