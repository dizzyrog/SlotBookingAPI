using SlotBookingAPI;

namespace SlotBooking.Domain;

public interface ISlotService
{
    public Task<UserSchedule> GetSlotsAsync(DateTime date);
}