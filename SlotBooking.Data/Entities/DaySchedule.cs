using Newtonsoft.Json;

namespace SlotBooking.Data.Entities;

public record DaySchedule
{
    [JsonProperty("WorkPeriod")] public WorkPeriod WorkPeriod { get; set; }

    [JsonProperty("BusySlots")] public List<BusySlot> BusySlots { get; set; } = new();
}