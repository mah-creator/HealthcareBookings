using System.Numerics;

namespace HealthcareBookings.Domain.Entities;

public class Appointment
{
    public string AppointmetnID { get; set; }
    public string Status { get; set; }
    public string DoctorID { get; set; }
    public Doctor Doctor { get; set; }
    public string PatientID { get; set; }
    public Patient Patient { get; set; }
    public string TimeSlotID { get; set; }
    public TimeSlot TimeSlot { get; set; }
    public AppointmentReview Review { get; set; }
}

public class AppointmentReview
{
    public string AppointmentID { get; set; }
    public float Rating { get; set; }
    public string ReviewText { get; set; }
}
