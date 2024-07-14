namespace SlotBooking.Domain.DTOs;

public record PatientDto
{
    public string Name { get; set; }
    public string SecondName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}