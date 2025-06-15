using EventSourcingFramework.Core.Interfaces;
using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Application.Mappers;
using SpotQuoteApp.Core.DomainObjects;
using SpotQuoteApp.Core.ValueObjects;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Application.Test.Integration;

public class EntityFactory
{
    private readonly IRepository<Customer> customerRepository;
    private readonly IRepository<Country> countryRepository;

    public EntityFactory(
        IRepository<Customer> customerRepository,
        IRepository<Country> countryRepository
    )
    {
        this.customerRepository = customerRepository;
        this.countryRepository = countryRepository;
    }

    public SpotQuoteDto CreateValidSpotQuote()
    {
        var customer = new Customer("Test");
        customerRepository.CreateAsync(customer).GetAwaiter().GetResult();
        var country = new Country("Denmark", "DK");
        countryRepository.CreateAsync(country).GetAwaiter().GetResult();

        return new SpotQuoteDto
        {
            Id = Guid.NewGuid(),
            AddressFrom = new AddressDto
            {
                City = "Copenhagen",
                Country = country.ToDto(),
                ZipCode = "1000",
            },
            AddressTo = new AddressDto
            {
                City = "Aarhus",
                Country = country.ToDto(),
                ZipCode = "8000",
            },
            ValidUntil = DateTime.UtcNow.AddDays(5),
            TransportMode = TransportMode.Road,
            Incoterm = Incoterm.DDP,
            Direction = Direction.Import,
            Customer = customer.ToDto(),
            ShippingDetails = new ShippingDetailsDto
            {
                Description = "Description",
                References = "References",
                ReadyToLoadDate = DateTime.UtcNow.AddDays(5),
                Collis = new List<ColliDto>
                {
                    new()
                    {
                        NumberOfUnits = 1,
                        Cbm = 1,
                        Height = 20,
                        Length = 40,
                        Type = ColliType.Box,
                        Weight = 200,
                        Width = 30,
                    },
                },
            },
            MailOptions = new MailOptions(true, true, "", new List<Guid>()).ToDto(
                new List<UserDto>()
            ),
            Quotes = new List<QuoteDto>
            {
                new()
                {
                    Supplier = Supplier.DHL,
                    Status = BookingStatus.Draft,
                    Profit = new ProfitDto(),
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
                                Total = 150,
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
                                Comment = "Margin",
                            },
                        },
                    },
                },
            },
        };
    }

    public static CustomerDto CreateCustomerDto(string name)
    {
        return new CustomerDto
        {
            Id = Guid.NewGuid(),
            Name = name,
            Users = new List<UserDto>(),
        };
    }
}
