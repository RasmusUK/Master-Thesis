using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuote.Application.Test.Integration;

public static class EntityFactory
{
    public static SpotQuoteDto CreateValidSpotQuote() => new()
    {
            AddressFrom = new AddressDto
            {
                City = "Copenhagen",
                Country = new CountryDto { Code = "DK", Name = "Denmark", Id = Guid.NewGuid() },
                ZipCode = "1000"
            },
            AddressTo = new AddressDto
            {
                City = "Aarhus",
                Country = new CountryDto { Code = "DK", Name = "Denmark", Id = Guid.NewGuid() },
                ZipCode = "8000"
            },
            ValidUntil = DateTime.UtcNow.AddDays(5),
            TransportMode = TransportMode.Road,
            Quotes = new List<QuoteDto>
            {
                new()
                {
                    Supplier = Supplier.DHL,
                    SupplierService = SupplierService.DHLExpress12,
                    ForwarderService = ForwarderService.DHLExpress,
                    Costs = new List<CostDto>
                    {
                        new()
                        {
                            SupplierCost = new SupplierCostDto
                            {
                                ChargeType = ChargeType.Freight,
                                CostType = CostType.PerShipment,
                                Value = 150,
                                Weight = 0,
                                Cbm = 0,
                                Total = 150
                            },
                            SellingCost = new SellingCostDto
                            {
                                ChargeType = ChargeType.Freight,
                                CostType = CostType.PerShipment,
                                Value = 200,
                                Weight = 0,
                                Cbm = 0,
                                Total = 200,
                                Profit = 50,
                                Comment = "Margin"
                            }
                        }
                    }
                }
            }
        };
}