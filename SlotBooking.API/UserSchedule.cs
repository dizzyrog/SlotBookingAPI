namespace SlotBooking.API;

public class UserSchedule
{
    public Facility Facility { get; set; }

    public DateTime WeekStartDate { get; set; }

    public List<DaySchedule> DaySchedules { get; set; } = new();
}

public class Facility
{
    public string FacilityId { get; set; }

    public string Name { get; set; }

    public string Address { get; set; }
}

public class DaySchedule
{
    public string DayOfWeek { get; set; }

    public List<AvailableSlot> AvailableSlots { get; set; }
}

public class AvailableSlot
{
    public DateTime Start { get; set; }

    public DateTime End { get; set; }
}

