using System.Globalization;
using EventSource.Infrastructure;
using FluentValidation;
using MudBlazor.Services;
using SpotQuoteBooking.EventSource.Application;
using SpotQuoteBooking.EventSource.Application.Interfaces;
using SpotQuoteBooking.EventSource.Core.AggregateRoots;
using SpotQuoteBooking.EventSource.Core.Validators;
using SpotQuoteBooking.EventSource.Web.Components;
using SpotQuoteBooking.EventSource.Web.Data;
using SpotQuoteBooking.EventSource.Web.Startup;
using Country = SpotQuoteBooking.EventSource.Core.AggregateRoots.Country;

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

RegistrationService.RegisterEntities(
    typeof(SpotQuote),
    typeof(Address),
    typeof(Customer),
    typeof(Country),
    typeof(Location),
    typeof(BuyingRate)
);

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
