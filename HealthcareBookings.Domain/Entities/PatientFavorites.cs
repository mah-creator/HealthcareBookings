namespace HealthcareBookings.Domain.Entities;


public class FavoriteDoctors
{
    public string PatientID { get; set; }
    public string DoctorID { get; set; }
    public Patient Patient { get; set; }
    public Doctor Doctor { get; set; }
}

public class FavoriteClinics
{
    public string PatientID { get; set; }
    public string ClinicID { get; set; }
    public Patient Patient { get; set; }
    public Clinic Clinic { get; set; }
}
