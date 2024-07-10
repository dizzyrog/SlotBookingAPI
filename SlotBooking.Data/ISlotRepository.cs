namespace SlotBooking.Data;

public interface ISlotRepository
{
    public Task<Schedule> GetScheduleAsync(DateTime date);
}