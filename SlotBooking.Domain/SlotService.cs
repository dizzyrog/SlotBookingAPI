using SlotBooking.Data;
using SlotBookingAPI;
using BusySlot = SlotBooking.Data.BusySlot;
using DaySchedule = SlotBooking.Data.DaySchedule;
using Facility = SlotBookingAPI.Facility;

namespace SlotBooking.Domain;

public class SlotService : ISlotService
{
    private readonly ISlotRepository _repository;

    public SlotService(ISlotRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserSchedule> GetSlotsAsync(DateTime date)
    {
        var schedule = await _repository.GetScheduleAsync(date);

        var availableSlotsSchedule = new UserSchedule()
        {
            Facility = new Facility()
            {
                FacilityId = schedule.Facility.FacilityId,
                Name = schedule.Facility.Name,
                Address = schedule.Facility.Address
            },
            WeekStartDate = date,
        };

        foreach (var daySchedule in schedule.DaySchedules)
        {
            var availableSlots = FindAvailableSlotsForDay(daySchedule.Value, schedule.SlotDurationMinutes);
            availableSlotsSchedule.DaySchedules.Add(new SlotBookingAPI.DaySchedule
            {
                DayOfWeek = daySchedule.Key.ToString(),
                AvailableSlots = availableSlots
            });
        }

        return availableSlotsSchedule;

    }

    private List<AvailableSlot> FindAvailableSlotsForDay(DaySchedule daySchedule, int slotDurationMinutes)
    {
        var busySlots = daySchedule.BusySlots;
        var availableSlots = new List<AvailableSlot>();
        var date = daySchedule.BusySlots.First().Start.Date;

        var startTime = new DateTime(date.Year, date.Month, date.Day, daySchedule.WorkPeriod.StartHour, 0, 0);
        var endTime = new DateTime(date.Year, date.Month, date.Day, daySchedule.WorkPeriod.EndHour, 0, 0);

        while (startTime < endTime)
        {
            var slotEndTime = startTime.AddMinutes(slotDurationMinutes);

            if (IsSlotAvailable(startTime, slotEndTime, daySchedule, busySlots))
            {
                availableSlots.Add(new AvailableSlot()
                {
                    Start = startTime,
                    End = slotEndTime
                });
            }

            startTime = slotEndTime;
        }

        return availableSlots;
    }

    private bool IsSlotAvailable(DateTime slotStart, DateTime slotEnd, DaySchedule daySchedule,
        List<BusySlot> busySlots)
    {
        var isLunchTime = slotStart.Hour >= daySchedule.WorkPeriod.LunchStartHour &&
                          slotStart.Hour < daySchedule.WorkPeriod.LunchEndHour;
        if (isLunchTime) return false;

        return !busySlots.Any(b => b.Start < slotEnd && b.End > slotStart);
    }

}