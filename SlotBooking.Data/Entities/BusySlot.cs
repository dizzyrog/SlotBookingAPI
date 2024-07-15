using Newtonsoft.Json;

namespace SlotBooking.Data.Entities;

public record BusySlot
{
    [JsonProperty("Start")] public DateTimeOffset Start { get; set; }

    [JsonProperty("End")] public DateTimeOffset End { get; set; }
}