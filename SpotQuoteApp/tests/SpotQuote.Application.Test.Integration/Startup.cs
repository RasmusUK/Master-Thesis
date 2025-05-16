using EventSourcingFramework.Infrastructure.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpotQuoteApp.Application.Interfaces;
using SpotQuoteApp.Application.Options;
using SpotQuoteApp.Application.Services;
using SpotQuoteApp.Core.AggregateRoots;
using SpotQuoteApp.Core.Validators;
using Country = SpotQuoteApp.Core.AggregateRoots.Country;

namespace SpotQuote.Application.Test.Integration;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();
        
        services.AddEventSourcing(configuration, (schema, migrations, migrator, mongoDbRegistrationService) =>
        {
            mongoDbRegistrationService.Register(
                (typeof(SpotQuoteApp.Core.AggregateRoots.SpotQuote), "SpotQuote"),
                (typeof(Address), "Address"),
                (typeof(Customer), "Customer"),
                (typeof(Country), "Country"),
                (typeof(Location), "Location"),
                (typeof(BuyingRate), "BuyingRate")
            );
        });

        services.Configure<MockApiOptions>(
            configuration.GetSection("MockApi"));
        
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<ISpotQuoteService, SpotQuoteService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IBuyingRateService, BuyingRateService>();
        services.AddScoped<SpotQuoteValidator, SpotQuoteValidator>();
    }
}