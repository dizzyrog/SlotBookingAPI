using Newtonsoft.Json;

namespace SlotBooking.Data.Entities;

public class DaySchedule
{
    [JsonProperty("WorkPeriod")]
    public WorkPeriod? WorkPeriod { get; set; }

    [JsonProperty("BusySlots")]
    public List<BusySlot> BusySlots { get; set; } = new ();
}