using EventSource.Infrastructure;
using MudBlazor.Services;
using SpotQuoteBooking.EventSource.Web.Components;
using SpotQuoteBooking.EventSource.Web.Startup;
using SpotQuoteBooking.Shared.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddEventSourcing(builder.Configuration);
builder.Services.AddSingleton<ICountryFetcher, CountryFetcher>();
builder.Services.AddSingleton<ISeeder, Seeder>();

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
    await seeder.SeedIfEmpty();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
