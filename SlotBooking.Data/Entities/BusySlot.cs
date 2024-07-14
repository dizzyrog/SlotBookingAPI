using Newtonsoft.Json;

namespace SlotBooking.Data.Entities;
public class BusySlot
{
    [JsonProperty("Start")]
    public DateTime Start { get; set; }

    [JsonProperty("End")]
    public DateTime End { get; set; }
}