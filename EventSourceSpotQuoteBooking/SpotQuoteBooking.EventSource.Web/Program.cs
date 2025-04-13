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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddEventSourcing(builder.Configuration);
builder.Services.AddSingleton<ICountryFetcher, CountryFetcher>();
builder.Services.AddSingleton<ISeeder, Seeder>();
builder.Services.AddSingleton<IAddressService, AddressService>();
builder.Services.AddSingleton<ICustomerService, CustomerService>();
builder.Services.AddSingleton<ICountryService, CountryService>();
builder.Services.AddSingleton<ISpotQuoteService, SpotQuoteService>();
builder.Services.AddSingleton<ILocationService, LocationService>();
builder.Services.AddSingleton<IBuyingRateService, BuyingRateService>();
builder.Services.AddSingleton<AbstractValidator<SpotQuote>, SpotQuoteValidator>();

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
    //await seeder.Seed();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

app.Run();
