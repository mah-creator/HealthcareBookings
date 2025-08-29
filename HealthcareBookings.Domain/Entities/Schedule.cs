namespace HealthcareBookings.Domain.Entities;

public class Schedule
{
    public string ScheduleID { get; set; }
    public string DoctorID { get; set; }
    public Doctor Doctor { get; set; }
    public DateOnly Date { get; set; }
    public bool IsAvailable { get; set; }
    public List<TimeSlot> TimeSlots { get; set; }
}

public class TimeSlot
{
    public string SlotID { get; set; }
    public string ScheduleID { get; set; }
    public Schedule Schedule { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}