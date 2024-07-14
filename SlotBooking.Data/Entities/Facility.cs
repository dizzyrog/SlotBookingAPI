using Newtonsoft.Json;

namespace SlotBooking.Data.Entities;

public record Facility
{
    [JsonProperty("FacilityId")]
    public string FacilityId { get; set; }

    [JsonProperty("Name")]
    public string? Name { get; set; }

    [JsonProperty("Address")]
    public string? Address { get; set; }
}