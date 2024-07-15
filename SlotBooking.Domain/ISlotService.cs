using SlotBooking.Domain.DTOs;

namespace SlotBooking.Domain;

/// <summary>
/// Calculating available slots and booking slots.
/// </summary>
public interface ISlotService
{
    /// <summary>
    /// Get available slots for a given week starting from specified date.
    /// </summary>
    /// <param name="date">The date representing the Monday from which the week's available slots are returned.</param>
    /// <returns>Week schedule with available slots. </returns>
    public Task<AvailableSlotsScheduleDto> GetAvailableSlotsAsync(DateTimeOffset date);
    /// <summary>
    /// Book an available slot for a patient.
    /// </summary>
    /// <param name="bookAvailableSlotDto">The slot and patient information.</param>
    public Task BookSlotAsync(BookAvailableSlotDto bookAvailableSlotDto);
}