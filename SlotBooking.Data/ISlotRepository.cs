using System.Net;
using SlotBooking.Data.Entities;

namespace SlotBooking.Data;

/// <summary>
/// Accessing external Slots service.
/// </summary>
public interface ISlotRepository
{
    /// <summary>
    /// Get busy slots for a given week starting from specified date.
    /// </summary>
    /// <param name="date">The date representing the Monday from which the week's busy slots are returned.</param>
    /// <returns>Week schedule with busy slots. </returns>
    public Task<BusySlotsSchedule> GetBusySlotsScheduleAsync(DateTimeOffset date);

    /// <summary>
    /// Book a slot for a patient.
    /// </summary>
    /// <param name="availableSlot">The slot and patient information.</param>
    /// <throws>HttpRequestException in case of a failure.</throws>
    public Task<HttpStatusCode> BookSlotAsync(AvailableSlot availableSlot);
}