using System.Numerics;

namespace HealthcareBookings.Domain.Entities;

public class Appointment
{
    public string AppointmentID { get; set; } = Guid.NewGuid().ToString();
    public string Status { get; set; } = AppointmentStatus.Upcoming;
    public string DoctorID { get; set; }
    public string PatientID { get; set; }
    public string TimeSlotID { get; set; }
    public Doctor Doctor { get; set; }
    public Patient Patient { get; set; }
    public TimeSlot TimeSlot { get; set; }
    public AppointmentReview Review { get; set; }
}

public class AppointmentReview
{
    public string AppointmentID { get; set; }
    public float Rating { get; set; } = 0;
    public string ReviewText { get; set; }
}
