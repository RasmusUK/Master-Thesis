using Microsoft.AspNetCore.Mvc;
using SpotQuoteBooking.EventSource.Application.Interfaces;
using SpotQuoteBooking.Shared.Dtos;

namespace SpotQuoteBooking.EventSource.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SpotQuoteBookingController : Controller
{
    private readonly ISpotQuoteBookingService spotQuoteBookingService;

    public SpotQuoteBookingController(ISpotQuoteBookingService spotQuoteBookingService)
    {
        this.spotQuoteBookingService = spotQuoteBookingService;
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> CreateSpotQuoteBooking(
        [FromBody] CreateSpotQuoteBookingDto createSpotQuoteBookingDto
    )
    {
        var id = await spotQuoteBookingService.CreateSpotQuoteBookingAsync(
            createSpotQuoteBookingDto
        );
        return Ok(id);
    }

    [HttpGet]
    [Route("[action]/{id}")]
    public async Task<IActionResult> GetSpotQuoteBookingById(Guid id)
    {
        var entity = await spotQuoteBookingService.GetSpotQuoteBookingByIdAsync(id);
        if (entity is null)
            return NotFound();
        return Ok(entity);
    }
}
