using EventSource.Core.Interfaces;
using SpotQuoteBooking.EventSource.Core;
using SpotQuoteBooking.Shared;
using SpotQuoteBooking.Shared.Data;

namespace SpotQuoteBooking.EventSource.Web.Startup;

public class Seeder : ISeeder
{
    private readonly IRepository<Customer> customerRepository;
    private readonly IRepository<Core.SpotQuoteBooking> spotQuoteBookingRepository;

    public Seeder(
        IRepository<Customer> customerRepository,
        IRepository<Core.SpotQuoteBooking> spotQuoteBookingRepository
    )
    {
        this.customerRepository = customerRepository;
        this.spotQuoteBookingRepository = spotQuoteBookingRepository;
    }

    public async Task SeedIfEmpty()
    {
        await SeedCustomersIfEmpty();
        await SeedSpotQuoteBookingsIfEmpty();
    }

    private async Task SeedCustomersIfEmpty()
    {
        if ((await customerRepository.ReadAllProjectionsAsync(x => x.Id)).Count > 0)
            return;
        for (var i = 1; i <= 10; i++)
        {
            var customer = new Customer($"Customer {i}");
            await customerRepository.CreateAsync(customer);
        }
    }

    private async Task SeedSpotQuoteBookingsIfEmpty()
    {
        if ((await spotQuoteBookingRepository.ReadAllProjectionsAsync(x => x.Id)).Count > 0)
            return;
        var addressFrom = new Address(
            "My company",
            new Country { Code = "DE", Name = "Germany" },
            "City",
            "2400",
            "Address 1",
            "Address 2",
            "mail@email.com",
            "+45 12345678",
            "Attention",
            null,
            null
        );

        var addressTo = new Address(
            "My company",
            new Country { Code = "DK", Name = "Denmark" },
            "Copenhagen NW",
            "2400",
            "Lærkevej 2",
            null,
            "mail@email.com",
            "+45 12345678",
            "Something",
            null,
            null
        );

        var shippingDetails = new ShippingDetails(
            new List<Colli> { new(1, ColliType.Box, 10, 20, 30, 200) },
            "Some description",
            DateTime.UtcNow.AddDays(2),
            "Some references"
        )
        {
            BookingProperties = new List<BookingProperty>
            {
                BookingProperty.ExportDeclaration,
                BookingProperty.NonStackable,
            },
        };

        for (var i = 0; i < 10; i++)
        {
            var spotQuoteBooking = new Core.SpotQuoteBooking
            {
                AddressFrom = addressFrom,
                AddressTo = addressTo,
                Direction = Direction.Import,
                TransportMode = TransportMode.Courier,
                Incoterm = Incoterm.DDP,
                ValidUntil = DateTime.UtcNow.AddDays(5),
                ShippingDetails = shippingDetails,
                Status = BookingStatus.Draft,
            };
            await spotQuoteBookingRepository.CreateAsync(spotQuoteBooking);
        }

        for (var i = 0; i < 10; i++)
        {
            addressFrom.Country = new Country { Code = "SE", Name = "Sweden" };
            addressTo.Country = new Country { Code = "NO", Name = "Norway" };
            var spotQuoteBooking = new Core.SpotQuoteBooking
            {
                AddressFrom = addressFrom,
                AddressTo = addressTo,
                Direction = Direction.Import,
                TransportMode = TransportMode.Road,
                Incoterm = Incoterm.FOB,
                ValidUntil = DateTime.UtcNow.AddDays(5),
                ShippingDetails = shippingDetails,
                Status = BookingStatus.SpotQuote,
            };
            await spotQuoteBookingRepository.CreateAsync(spotQuoteBooking);
        }

        for (var i = 0; i < 10; i++)
        {
            addressFrom.Country = new Country { Code = "PL", Name = "Poland" };
            addressFrom.Airport = "Airport";
            addressTo.Country = new Country { Code = "GB", Name = "Great Britain" };
            addressTo.Port = "Port";
            var spotQuoteBooking = new Core.SpotQuoteBooking
            {
                AddressFrom = addressFrom,
                AddressTo = addressTo,
                Direction = Direction.Import,
                TransportMode = TransportMode.Air,
                Incoterm = Incoterm.FCA,
                ValidUntil = DateTime.UtcNow.AddDays(5),
                ShippingDetails = shippingDetails,
                Status = BookingStatus.Accepted,
            };
            await spotQuoteBookingRepository.CreateAsync(spotQuoteBooking);
        }

        for (var i = 0; i < 10; i++)
        {
            var spotQuoteBooking = new Core.SpotQuoteBooking
            {
                AddressFrom = addressFrom,
                AddressTo = addressTo,
                Direction = Direction.Export,
                TransportMode = TransportMode.Courier,
                Incoterm = Incoterm.EXW,
                ValidUntil = DateTime.UtcNow.AddDays(5),
                ShippingDetails = shippingDetails,
                Status = BookingStatus.Requote,
            };
            await spotQuoteBookingRepository.CreateAsync(spotQuoteBooking);
        }

        for (var i = 0; i < 10; i++)
        {
            addressFrom.Country = new Country { Code = "SE", Name = "Sweden" };
            addressTo.Country = new Country { Code = "NO", Name = "Norway" };
            var spotQuoteBooking = new Core.SpotQuoteBooking
            {
                AddressFrom = addressFrom,
                AddressTo = addressTo,
                Direction = Direction.Export,
                TransportMode = TransportMode.Road,
                Incoterm = Incoterm.CIF,
                ValidUntil = DateTime.UtcNow.AddDays(5),
                ShippingDetails = shippingDetails,
                Status = BookingStatus.NotAccepted,
            };
            await spotQuoteBookingRepository.CreateAsync(spotQuoteBooking);
        }

        for (var i = 0; i < 10; i++)
        {
            addressFrom.Country = new Country { Code = "PL", Name = "Poland" };
            addressFrom.Port = "Port";
            addressTo.Country = new Country { Code = "GB", Name = "Great Britain" };
            addressTo.Port = "Port";
            var spotQuoteBooking = new Core.SpotQuoteBooking
            {
                AddressFrom = addressFrom,
                AddressTo = addressTo,
                Direction = Direction.Export,
                TransportMode = TransportMode.Sea,
                Incoterm = Incoterm.DAP,
                ValidUntil = DateTime.UtcNow.AddDays(5),
                ShippingDetails = shippingDetails,
                Status = BookingStatus.Requote,
            };
            await spotQuoteBookingRepository.CreateAsync(spotQuoteBooking);
        }
    }
}
