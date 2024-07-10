using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SlotBooking.Data;

public class Schedule
{
    [JsonProperty("Facility")]
    public Facility Facility { get; set; }

    [JsonProperty("SlotDurationMinutes")]
    public int SlotDurationMinutes { get; set; }
    
    [JsonIgnore]
    public Dictionary<DayOfWeek, DaySchedule> DaySchedules { get; set; } = new Dictionary<DayOfWeek, DaySchedule>();

    [JsonExtensionData]
    private IDictionary<string, JToken> _additionalData;
}


public class Facility
{
    [JsonProperty("FacilityId")]
    public string FacilityId { get; set; }

    [JsonProperty("Name")]
    public string Name { get; set; }

    [JsonProperty("Address")]
    public string Address { get; set; }
}

public class BusySlot
{
    [JsonProperty("Start")]
    public DateTime Start { get; set; }

    [JsonProperty("End")]
    public DateTime End { get; set; }
}

public class DaySchedule
{
    [JsonProperty("WorkPeriod")]
    public WorkPeriod WorkPeriod { get; set; }

    [JsonProperty("BusySlots")]
    public List<BusySlot> BusySlots { get; set; }
}


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