using System.Globalization;
using EventSourcingFramework.Infrastructure.DI;
using MudBlazor.Services;
using SpotQuoteApp.Application;
using SpotQuoteApp.Application.Interfaces;
using SpotQuoteApp.Application.Options;
using SpotQuoteApp.Application.Services;
using SpotQuoteApp.Core.DomainObjects;
using SpotQuoteApp.Core.DomainObjects.Old;
using SpotQuoteApp.Core.Validators;
using SpotQuoteApp.Web.Components;
using SpotQuoteApp.Web.Data;
using SpotQuoteApp.Web.Startup;
using Country = SpotQuoteApp.Core.DomainObjects.Country;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddEventSourcing(
    builder.Configuration,
    (schema, migrations, migrator, mongoDbRegistrationService) =>
    {
        mongoDbRegistrationService.Register(
            (typeof(SpotQuote), "SpotQuote"),
            (typeof(Address), "Address"),
            (typeof(Customer), "Customer"),
            (typeof(Country), "Country"),
            (typeof(Location), "Location"),
            (typeof(BuyingRate), "BuyingRate")
        );

        schema.Register(typeof(SpotQuote), 2);
        migrations.Register<SpotQuote>(1, typeof(SpotQuoteV1));
        migrations.Register<SpotQuote>(2, typeof(SpotQuote));

        migrator.Register<SpotQuoteV1, SpotQuote>(
            1,
            v1 => new SpotQuote(
                v1.AddressFromId,
                v1.AddressToId,
                v1.Direction,
                v1.TransportMode,
                v1.Incoterm,
                v1.ShippingDetails,
                v1.ValidUntil,
                v1.Customer.Id,
                v1.MailOptions,
                v1.InternalComments,
                v1.Quotes
            )
        );
    }
);

builder.Services.Configure<MockApiOptions>(builder.Configuration.GetSection("MockApi"));

builder.Services.AddScoped<ICountryFetcher, CountryFetcher>();
builder.Services.AddScoped<ISeeder, Seeder>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<ISpotQuoteService, SpotQuoteService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IBuyingRateService, BuyingRateService>();
builder.Services.AddScoped<SpotQuoteValidator, SpotQuoteValidator>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

using var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
await seeder.Seed();

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

app.Run();
