namespace SlotBooking.Domain.DTOs;

public class AvailableSlotsScheduleDto
{
    public FacilityDto? FacilityDto { get; set; }

    public DateTime WeekStartDate { get; set; }

    public List<DayScheduleDto> DaySchedules { get; set; } = new ();
}