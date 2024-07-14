namespace SlotBooking.Domain.DTOs;

public record DayScheduleDto
{
    public string DayOfWeek { get; set; }

    public List<AvailableSlotDto> AvailableSlots { get; set; } = new();
}