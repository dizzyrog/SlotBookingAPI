namespace SlotBooking.Domain.DTOs;

public class DayScheduleDto
{
    public string? DayOfWeek { get; set; }

    public List<AvailableSlotDto> AvailableSlots { get; set; } = new();
}