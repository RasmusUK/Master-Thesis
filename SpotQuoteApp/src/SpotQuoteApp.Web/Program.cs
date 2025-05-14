using System.Globalization;
using EventSourcingFramework.Infrastructure.Abstractions.Migrations;
using EventSourcingFramework.Infrastructure.Abstractions.MongoDb;
using EventSourcingFramework.Infrastructure.DependencyInjection;
using EventSourcingFramework.Infrastructure.Migrations;
using EventSourcingFramework.Infrastructure.MongoDb;
using MudBlazor.Services;
using SpotQuoteApp.Application;
using SpotQuoteApp.Application.Interfaces;
using SpotQuoteApp.Core.AggregateRoots;
using SpotQuoteApp.Core.Validators;
using SpotQuoteApp.Web.Components;
using SpotQuoteApp.Web.Data;
using SpotQuoteApp.Web.Startup;
using Country = SpotQuoteApp.Core.AggregateRoots.Country;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddEventSourcing(builder.Configuration);

builder.Services.AddScoped<ICountryFetcher, CountryFetcher>();
builder.Services.AddScoped<ISeeder, Seeder>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<ISpotQuoteService, SpotQuoteService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IBuyingRateService, BuyingRateService>();
builder.Services.AddScoped<SpotQuoteValidator, SpotQuoteValidator>();

builder.Services.AddSingleton<ISchemaVersionRegistry>(_ =>
{
    var registry = new SchemaVersionRegistry();
    return registry;
});

builder.Services.AddSingleton<IMigrationTypeRegistry>(_ =>
{
    var registry = new MigrationTypeRegistry();
    return registry;
});

builder.Services.AddSingleton<IEntityMigrator>(_ =>
{
    var migrator = new EntityMigrator();
    return migrator;
});

builder.Services.AddSingleton<IEntityCollectionNameProvider>(sp =>
{
    var registry = sp.GetRequiredService<IMigrationTypeRegistry>();
    var collectionNameProvider = new EntityCollectionNameProvider(registry);

    MongoDbEventRegistration.RegisterEntities(
        collectionNameProvider,
        (typeof(SpotQuote), "SpotQuote"),
        (typeof(Address), "Address"),
        (typeof(Customer), "Customer"),
        (typeof(Country), "Country"),
        (typeof(Location), "Location"),
        (typeof(BuyingRate), "BuyingRate")
    );

    return collectionNameProvider;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
    await seeder.Seed();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

app.Run();
