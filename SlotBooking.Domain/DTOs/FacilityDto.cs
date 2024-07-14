namespace SlotBooking.Domain.DTOs;

public record FacilityDto
{
    public string FacilityId { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }
}