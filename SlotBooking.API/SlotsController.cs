using Microsoft.AspNetCore.Mvc;
using SlotBooking.Domain;

namespace SlotBooking.API;

[ApiController]
[Route("api/[controller]")]
public class SlotsController : ControllerBase
{
    private readonly ISlotService _slotService;
    public SlotsController(ISlotService slotService)
    {
        _slotService = slotService;
    }
    
    [HttpGet("{date}")]
    public async Task<ActionResult<IEnumerable<DateTime>>> GetAvaliableSlots(DateTime date)
    {
        //error hadnling
        //validation
        
        return Ok(await _slotService.GetSlotsAsync(date));
    }
}