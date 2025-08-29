namespace HealthcareBookings.Domain.Entities;

public class Doctor
{
    public string DoctorUID { get; set; }
    public string? Bio { get; set; }
    public int ExperienceYears { get; set; }
    public float Rating { get; set; }
    public string ClinicID { get; set; }
    public Clinic Clinic { get; set; }
    public string CategoryID { get; set; }
    public DoctorCategory Category { get; set; }
    public List<Schedule> Schedules { get; set; }
    public List<Appointment> Appointments { get; set; }
}

public class DoctorCategory
{
    public string CategoryID { get; set; } = Guid.NewGuid().ToString();
    public string CategoryName { get; set; }
    public string? CategoryDescription { get; set; }
    public string? CategoryLogoPath { get; set; }
    public List<Doctor> Doctors { get; set; }
}