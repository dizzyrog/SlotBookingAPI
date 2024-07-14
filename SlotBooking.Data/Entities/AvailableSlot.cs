namespace SlotBooking.Data.Entities;

public record AvailableSlot
{
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public string? Comments { get; set; }
    public string FacilityId { get; set; }
    public Patient Patient { get; set; }
}