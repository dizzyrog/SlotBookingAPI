using Microsoft.AspNetCore.Mvc;
using SlotBooking.Domain;
using SlotBooking.Domain.DTOs;

namespace SlotBooking.API;

/// <summary>
/// Slots controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SlotsController : ControllerBase
{
    private readonly ISlotService _slotService;

    /// <summary>
    /// Constructor for initializing a <see cref="SlotsController"/> class instance.
    /// </summary>
    /// <param name="slotService">Post service</param>
    public SlotsController(ISlotService slotService)
    {
        _slotService = slotService;
    }

    /// <summary>
    /// Gets all available slots for a week starting from the specified date.
    /// </summary>
    /// <param name="date">The date representing the Monday from which the week's available slots are returned.</param>
    /// <returns></returns>
    [HttpGet("{date}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AvailableSlotsScheduleDto>> GetAvailableSlots(DateTimeOffset date)
    {
        var availableSlotsScheduleDto = await _slotService.GetAvailableSlotsAsync(date);
        return Ok(availableSlotsScheduleDto);
    }

    /// <summary>
    /// Book the specified slot for the patient.
    /// </summary>
    /// <param name="bookAvailableSlotDto">AvailableSlot details.</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> PostSlot(BookAvailableSlotDto bookAvailableSlotDto)
    {
        await _slotService.BookSlotAsync(bookAvailableSlotDto);
        return Ok();
    }
}