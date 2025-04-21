using EventSource.Core.Interfaces;
using EventSource.Persistence.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SpotQuoteBooking.EventSource.Application.DTOs;
using SpotQuoteBooking.EventSource.Core.AggregateRoots;
using SpotQuoteBooking.EventSource.Core.ValueObjects;
using SpotQuoteBooking.EventSource.Core.ValueObjects.Enums;
using SpotQuoteBooking.EventSource.Web.Data;
using Country = SpotQuoteBooking.EventSource.Core.AggregateRoots.Country;

namespace SpotQuoteBooking.EventSource.Web.Startup;

public class Seeder : ISeeder
{
    private readonly ICountryFetcher countryFetcher;
    private readonly IRepository<Customer> customerRepository;
    private readonly IRepository<SpotQuote> spotQuoteBookingRepository;
    private readonly IRepository<Country> countryRepository;
    private readonly IRepository<Address> addressRepository;
    private readonly IRepository<Location> locationRepository;
    private readonly IRepository<BuyingRate> buyingRateRepository;
    private readonly IMongoDbService mongoDbService;
    private const int Nr = 10;

    public Seeder(
        ICountryFetcher countryFetcher,
        IRepository<Customer> customerRepository,
        IRepository<SpotQuote> spotQuoteBookingRepository,
        IRepository<Country> countryRepository,
        IRepository<Address> addressRepository,
        IRepository<Location> locationRepository,
        IRepository<BuyingRate> buyingRateRepository,
        IMongoDbService mongoDbService
    )
    {
        this.countryFetcher = countryFetcher;
        this.customerRepository = customerRepository;
        this.spotQuoteBookingRepository = spotQuoteBookingRepository;
        this.countryRepository = countryRepository;
        this.addressRepository = addressRepository;
        this.locationRepository = locationRepository;
        this.mongoDbService = mongoDbService;
        this.buyingRateRepository = buyingRateRepository;
    }

    public async Task Seed()
    {
        await DeleteAll();
        await SeedCustomers();
        await SeedCountries();
        await SeedAddresses();
        await SeedLocations();
        await SeedBuyingRates();
        await SeedSpotQuoteBookings();
    }

    private Task DeleteAll() => mongoDbService.CleanUpAsync();

    private async Task SeedCustomers()
    {
        for (var i = 1; i <= Nr; i++)
        {
            var customer = new Customer($"Customer {i}");
            await customerRepository.CreateAsync(customer);
        }
    }

    private async Task SeedCountries()
    {
        foreach (var country in countryFetcher.GetCountries())
        {
            var domainCountry = new Country(country.Name, country.Code);
            await countryRepository.CreateAsync(domainCountry);
        }
    }

    private async Task SeedAddresses()
    {
        var countries = (await countryRepository.ReadAllAsync()).ToList();
        for (var i = 1; i <= Nr; i++)
        {
            var address = new Address(
                $"Company {i}",
                countries[i - 1].Id,
                $"City {i}",
                $"ZipCode {i}",
                $"Address {i}",
                $"Address {i}",
                $"Email {i}",
                $"Phone {i}",
                $"Attention {i}"
            );
            await addressRepository.CreateAsync(address);
        }
    }

    private async Task SeedLocations()
    {
        var countries = (await countryRepository.ReadAllAsync()).ToList();
        for (var i = 1; i <= Nr; i++)
        {
            var locationType =
                i % 3 == 0 ? LocationType.Port
                : i % 3 == 1 ? LocationType.Airport
                : LocationType.ZipCode;
            var location = new Location(
                $"LocationCode {i}",
                $"LocationName {i}",
                countries[i - 1].Id,
                locationType
            );
            await locationRepository.CreateAsync(location);
        }
    }

    private async Task SeedBuyingRates()
    {
        var locations = (await locationRepository.ReadAllAsync()).ToList();
        for (var i = 1; i <= Nr; i++)
        {
            var transportMode =
                i % 4 == 0 ? TransportMode.Air
                : i % 4 == 1 ? TransportMode.Courier
                : i % 4 == 2 ? TransportMode.Sea
                : TransportMode.Road;
            var supplier =
                i % 4 == 0 ? Supplier.Aramex
                : i % 4 == 1 ? Supplier.Maersk
                : i % 4 == 2 ? Supplier.FedEx
                : Supplier.DHL;
            var forwarderService = ForwarderService.GetBySupplier(supplier).First();
            var supplierService = SupplierService.GetByForwarderService(forwarderService).First();
            var buyingRate = new BuyingRate(
                transportMode,
                supplier,
                supplierService,
                forwarderService,
                locations[i - 1].Id,
                locations[(i + 2) % Nr].Id,
                DateTime.UtcNow.AddYears(-1),
                DateTime.UtcNow.AddYears(1),
                new SupplierCost(ChargeType.Delivery, CostType.PerKg, 100, 10, 10)
            );

            await buyingRateRepository.CreateAsync(buyingRate);
        }
    }

    private async Task SeedSpotQuoteBookings()
    {
        var addresses = (await addressRepository.ReadAllAsync()).ToList();
        var customers = (await customerRepository.ReadAllAsync()).ToList();
        for (var i = 1; i <= Nr; i++)
        {
            var transportMode =
                i % 4 == 0 ? TransportMode.Air
                : i % 4 == 1 ? TransportMode.Courier
                : i % 4 == 2 ? TransportMode.Sea
                : TransportMode.Road;
            var incoterm = Incoterm.GetAll().ToList()[i % 10];
            var bookingStatus =
                i % 6 == 0 ? BookingStatus.SpotQuote
                : i % 6 == 1 ? BookingStatus.Draft
                : i % 6 == 2 ? BookingStatus.Requote
                : i % 6 == 3 ? BookingStatus.Accepted
                : i % 6 == 4 ? BookingStatus.NotAccepted
                : BookingStatus.PendingSubmit;
            var spotQuoteBooking = new SpotQuote(
                addresses[i - 1].Id,
                addresses[(i + 2) % Nr].Id,
                i % 2 == 0 ? Direction.Export : Direction.Import,
                transportMode,
                incoterm,
                bookingStatus,
                new ShippingDetails(
                    $"Description {i}",
                    $"References {i}",
                    DateTime.UtcNow.AddDays(i)
                ),
                DateTime.UtcNow.AddDays(14 + i),
                customers[i - 1].Id,
                new MailOptions(true, false, $"Comment {i}", new List<Guid>()),
                $"Internal comments {i}",
                new List<Quote>()
            );
            await spotQuoteBookingRepository.CreateAsync(spotQuoteBooking);
        }
    }
}
