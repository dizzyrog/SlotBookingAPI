namespace SlotBooking.Data.Entities;

public class AvailableSlot
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Comments { get; set; }
    public string? FacilityId { get; set; }
    public Patient? Patient { get; set; }
}