namespace SlotBooking.Data;

/// <summary>
/// Class to hold the configuration for the slot service, implementation of Options pattern.
/// </summary>
public class SlotServiceOptions
{
    public const string SlotService = "SlotService";
    public string BaseUrl { get; set; } = String.Empty;
    public string Username { get; set; }= String.Empty;
    public string Password { get; set; }= String.Empty;
}