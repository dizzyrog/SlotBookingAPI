using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SlotBooking.Data.Entities;

public record BusySlotsSchedule
{
    [JsonProperty("Facility")]
    public Facility Facility { get; set; }

    [JsonProperty("SlotDurationMinutes")]
    public int SlotDurationMinutes { get; set; }
    
    [JsonIgnore]
    public Dictionary<DayOfWeek, DaySchedule> DaySchedules { get; set; } = new ();

    [JsonExtensionData]
    private IDictionary<string, JToken> _additionalData;
}