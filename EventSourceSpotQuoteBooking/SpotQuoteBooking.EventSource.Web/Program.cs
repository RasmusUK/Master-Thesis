using EventSource.Core.Interfaces;
using EventSource.Infrastructure;
using SpotQuoteBooking.EventSource.Application;
using SpotQuoteBooking.EventSource.Application.Interfaces;
using SpotQuoteBooking.EventSource.Core;
using SpotQuoteBooking.EventSource.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddControllers();
builder.Services.AddEventSourcing(builder.Configuration);

builder.Services.AddSingleton<ISpotQuoteBookingService, SpotQuoteBookingService>();

var app = builder.Build();

var eventProcessor = app.Services.GetRequiredService<IEventProcessor>();
eventProcessor.RegisterEventToEntity<
    CreateSpotQuoteBookingEvent,
    SpotQuoteBooking.EventSource.Core.SpotQuoteBooking
>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.UseRouting();

app.UseAntiforgery();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapBlazorHub();
    endpoints.MapRazorPages();
});

app.Run();
