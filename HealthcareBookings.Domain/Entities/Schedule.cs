namespace HealthcareBookings.Domain.Entities;

public class Schedule
{
    public string ScheduleID { get; set; } = Guid.NewGuid().ToString();
    public string DoctorID { get; set; }
    //public Doctor Doctor { get; set; }
    public DateOnly Date { get; set; }
    public bool IsAvailable { get; set; } = true;
    public List<TimeSlot> TimeSlots { get; set; }
}

public class TimeSlot
{
    public string SlotID { get; set; } = Guid.NewGuid().ToString();
    public string ScheduleID { get; set; }
    public Schedule Schedule { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsFree { get; set; } = true;
}