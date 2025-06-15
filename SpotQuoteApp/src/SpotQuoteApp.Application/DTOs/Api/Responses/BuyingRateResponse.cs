namespace SpotQuoteApp.Application.DTOs.Api.Responses;

public class BuyingRateResponse
{
    public string Supplier { get; set; }
    public string ForwarderService { get; set; }
    public string SupplierService { get; set; }
    public string CountryFrom { get; set; } 
    public string CountryTo { get; set; }
    public string TransportMode { get; set; }
    public double Price { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public string ChargeType { get; set; }
    public string CostType { get; set; }
}
