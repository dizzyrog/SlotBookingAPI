using System.Net;
using SlotBooking.Data;
using SlotBooking.Data.Entities;
using SlotBooking.Domain.DTOs;
using AvailableSlot = SlotBooking.Data.Entities.AvailableSlot;
using BusySlot = SlotBooking.Data.Entities.BusySlot;
using DaySchedule = SlotBooking.Data.Entities.DaySchedule;

namespace SlotBooking.Domain;

/// <inheritdoc />
public class SlotService(ISlotRepository repository) : ISlotService
{
    /// <inheritdoc />
    public async Task<AvailableSlotsScheduleDto> GetAvailableSlotsAsync(DateTime date)
    {
        var schedule = await repository.GetBusySlotsAsync(date);

        var availableSlotsSchedule = new AvailableSlotsScheduleDto()
        {
            FacilityDto = new FacilityDto()
            {
                FacilityId = schedule.Facility?.FacilityId,
                Name = schedule.Facility?.Name,
                Address = schedule.Facility?.Address
            },
            WeekStartDate = date,
        };

        foreach (var daySchedule in schedule.DaySchedules)
        {
            DateTime dateForDayOfWeek = date;
            
            if (daySchedule.Key != DayOfWeek.Monday)
            {
                dateForDayOfWeek = GetDateForDayOfWeek(date, daySchedule.Key);
            }
            
            var availableSlots = FindAvailableSlotsForDay(daySchedule.Value,dateForDayOfWeek, schedule.SlotDurationMinutes);
            
            availableSlotsSchedule.DaySchedules.Add(new DayScheduleDto
            {
                DayOfWeek = daySchedule.Key.ToString(),
                AvailableSlots = availableSlots
            });
        }

        return availableSlotsSchedule;

    }
    
    /// <inheritdoc />
    public Task<HttpStatusCode> CreateSlotAsync(BookAvailableSlotDto bookAvailableSlotDto)
    {
        var slot = new AvailableSlot()
        {
            FacilityId = bookAvailableSlotDto.FacilityId,
            Start = bookAvailableSlotDto.Start,
            End = bookAvailableSlotDto.End,
            Comments = bookAvailableSlotDto.Comments,
            Patient = new Patient()
            {
                Email = bookAvailableSlotDto.Patient?.Email,
                Name = bookAvailableSlotDto.Patient?.Name,
                Phone = bookAvailableSlotDto.Patient?.Phone,
                SecondName = bookAvailableSlotDto.Patient?.SecondName
            }
        };      
        
        return repository.PostSlotAsync(slot);
    }

    private DateTime GetDateForDayOfWeek(DateTime mondayDate, DayOfWeek targetDay)
    {
        int daysDifference = ((int)targetDay - (int)DayOfWeek.Monday + 7) % 7;
        return mondayDate.AddDays(daysDifference);
    }
    
    private List<AvailableSlotDto> FindAvailableSlotsForDay(DaySchedule daySchedule, DateTime date, int slotDurationMinutes)
    {
        var busySlots = daySchedule.BusySlots;
        var availableSlots = new List<AvailableSlotDto>();

        var startTime = new DateTime(date.Year, date.Month, date.Day, daySchedule.WorkPeriod.StartHour, 0, 0);
        var endTime = new DateTime(date.Year, date.Month, date.Day, daySchedule.WorkPeriod.EndHour, 0, 0);

        while (startTime < endTime)
        {
            var slotEndTime = startTime.AddMinutes(slotDurationMinutes);

            if (IsSlotAvailable(startTime, slotEndTime, daySchedule, busySlots))
            {
                availableSlots.Add(new AvailableSlotDto()
                {
                    Start = startTime,
                    End = slotEndTime
                });
            }

            startTime = slotEndTime;
        }

        return availableSlots;
    }
    
    private bool IsSlotAvailable(DateTime slotStart, DateTime slotEnd, DaySchedule daySchedule, List<BusySlot> busySlots)
    {
        var isLunchTime = slotStart.Hour >= daySchedule.WorkPeriod?.LunchStartHour &&
                          slotStart.Hour < daySchedule.WorkPeriod.LunchEndHour;
        if (isLunchTime) return false;
        
        return !busySlots.Any(b => b.Start < slotEnd && b.End > slotStart);
    }
}