namespace SlotBooking.Domain.DTOs;

public class BookAvailableSlotDto
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Comments { get; set; }
    public string? FacilityId { get; set; }
    public PatientDto? Patient { get; set; }
}