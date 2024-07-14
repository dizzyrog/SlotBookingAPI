namespace SlotBooking.Domain.DTOs;

public record AvailableSlotDto
{
    public DateTimeOffset Start { get; set; }

    public DateTimeOffset End { get; set; }
}