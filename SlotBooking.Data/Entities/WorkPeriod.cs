using Newtonsoft.Json;

namespace SlotBooking.Data.Entities;

public class WorkPeriod
{
    [JsonProperty("StartHour")]
    public int StartHour { get; set; }

    [JsonProperty("EndHour")]
    public int EndHour { get; set; }

    [JsonProperty("LunchStartHour")]
    public int LunchStartHour { get; set; }

    [JsonProperty("LunchEndHour")]
    public int LunchEndHour { get; set; }
}