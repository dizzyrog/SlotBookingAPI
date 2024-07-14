namespace SlotBooking.Domain.DTOs;

public record AvailableSlotsScheduleDto
{
    public FacilityDto FacilityDto { get; set; }

    public DateTimeOffset WeekStartDate { get; set; }

    public List<DayScheduleDto> DaySchedules { get; set; } = new ();
}