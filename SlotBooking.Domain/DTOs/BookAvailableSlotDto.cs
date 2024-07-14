namespace SlotBooking.Domain.DTOs;

public record BookAvailableSlotDto
{
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public string? Comments { get; set; }
    public string FacilityId { get; set; }
    public PatientDto Patient { get; set; }
}