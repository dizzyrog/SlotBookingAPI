using FluentValidation;
using Microsoft.Extensions.Logging;
using SlotBooking.Data;
using SlotBooking.Data.Entities;
using SlotBooking.Domain.DTOs;
using SlotBooking.Domain.Exceptions;
using AvailableSlot = SlotBooking.Data.Entities.AvailableSlot;
using BusySlot = SlotBooking.Data.Entities.BusySlot;
using DaySchedule = SlotBooking.Data.Entities.DaySchedule;

namespace SlotBooking.Domain;

/// <inheritdoc />
public class SlotService(
    ISlotRepository repository,
    IValidator<DateTimeOffset> dateTimeOffsetValidator,
    ILogger<SlotService> logger) : ISlotService
{
    /// <inheritdoc />
    public async Task<AvailableSlotsScheduleDto> GetAvailableSlotsAsync(DateTimeOffset date)
    {
        await dateTimeOffsetValidator.ValidateAndThrowAsync(date);

        var busySlotsSchedule = await repository.GetBusySlotsScheduleAsync(date);

        ArgumentNullException.ThrowIfNull(busySlotsSchedule, nameof(busySlotsSchedule));
        ArgumentNullException.ThrowIfNull(busySlotsSchedule.Facility, nameof(busySlotsSchedule.Facility));

        var availableSlotsSchedule = new AvailableSlotsScheduleDto()
        {
            FacilityDto = new FacilityDto()
            {
                FacilityId = busySlotsSchedule.Facility.FacilityId,
                Name = busySlotsSchedule.Facility.Name,
                Address = busySlotsSchedule.Facility.Address
            },
            WeekStartDate = date,
        };

        foreach (var (dayOfWeek, daySchedule) in busySlotsSchedule.DaySchedules)
        {
            DateTimeOffset dateForDayOfWeek = date;

            if (dayOfWeek != DayOfWeek.Monday)
            {
                dateForDayOfWeek = GetDateForDayOfWeek(date, dayOfWeek);
            }

            var availableSlots =
                FindAvailableSlotsForDay(daySchedule, dateForDayOfWeek, busySlotsSchedule.SlotDurationMinutes);

            availableSlotsSchedule.DaySchedules.Add(new DayScheduleDto
            {
                DayOfWeek = dayOfWeek.ToString(),
                AvailableSlots = availableSlots
            });
        }

        return availableSlotsSchedule;
    }

    /// <inheritdoc />
    public async Task BookSlotAsync(BookAvailableSlotDto bookAvailableSlotDto)
    {
        await ValidateSlotIsAvailable(bookAvailableSlotDto);

        var slot = new AvailableSlot()
        {
            FacilityId = bookAvailableSlotDto.FacilityId,
            Start = bookAvailableSlotDto.Start,
            End = bookAvailableSlotDto.End,
            Comments = bookAvailableSlotDto.Comments,
            Patient = new Patient()
            {
                Email = bookAvailableSlotDto.Patient.Email,
                Name = bookAvailableSlotDto.Patient.Name,
                Phone = bookAvailableSlotDto.Patient.Phone,
                SecondName = bookAvailableSlotDto.Patient.SecondName
            }
        };

        try
        {
            await repository.BookSlotAsync(slot);
        }
        catch (HttpRequestException e)
        {
            logger.LogError(e, "Failed to book a slot");
            throw new FailedToBookAvailableSlotException($"Failed to book a slot. {e.HttpRequestError.ToString()}",
                e.InnerException);
        }
    }

    private async Task ValidateSlotIsAvailable(BookAvailableSlotDto bookAvailableSlotDto)
    {
        var weekStartDate = GetMondayOfWeek(bookAvailableSlotDto.Start);
        var availableSlotsSchedule = await GetAvailableSlotsAsync(weekStartDate);
        var daySchedule =
            availableSlotsSchedule.DaySchedules.Find(
                x => x.DayOfWeek == bookAvailableSlotDto.Start.DayOfWeek.ToString());
        if (daySchedule == null ||
            !daySchedule.AvailableSlots.Any(x =>
                x.Start == bookAvailableSlotDto.Start && x.End == bookAvailableSlotDto.End))
        {
            throw new SlotNotAvailableException("The slot is not available.");
        }
    }

    private DateTimeOffset GetMondayOfWeek(DateTimeOffset date)
    {
        int daysToSubtract = (int) date.DayOfWeek - (int) DayOfWeek.Monday;
        if (daysToSubtract < 0)
        {
            daysToSubtract += 7;
        }

        return date.AddDays(-daysToSubtract).Date;
    }

    private DateTimeOffset GetDateForDayOfWeek(DateTimeOffset mondayDate, DayOfWeek targetDay)
    {
        int daysDifference = ((int) targetDay - (int) DayOfWeek.Monday + 7) % 7;
        return mondayDate.AddDays(daysDifference);
    }

    private List<AvailableSlotDto> FindAvailableSlotsForDay(DaySchedule daySchedule, DateTimeOffset date,
        int slotDurationMinutes)
    {
        var busySlots = daySchedule.BusySlots;
        var availableSlots = new List<AvailableSlotDto>();

        ArgumentNullException.ThrowIfNull(daySchedule.WorkPeriod, nameof(daySchedule.WorkPeriod));

        var startTime = new DateTimeOffset(date.Year, date.Month, date.Day, daySchedule.WorkPeriod.StartHour, 0, 0,
            DateTimeOffset.Now.Offset);
        var endTime = new DateTimeOffset(date.Year, date.Month, date.Day, daySchedule.WorkPeriod.EndHour, 0, 0,
            DateTimeOffset.Now.Offset);

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

    private bool IsSlotAvailable(DateTimeOffset slotStart, DateTimeOffset slotEnd, DaySchedule daySchedule,
        IEnumerable<BusySlot> busySlots)
    {
        var isLunchTime = slotStart.Hour >= daySchedule.WorkPeriod?.LunchStartHour &&
                          slotStart.Hour < daySchedule.WorkPeriod.LunchEndHour;
        if (isLunchTime) return false;

        return !busySlots.Any(b => b.Start < slotEnd && b.End > slotStart);
    }
}