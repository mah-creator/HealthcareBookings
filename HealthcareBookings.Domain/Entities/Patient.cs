namespace HealthcareBookings.Domain.Entities;

public class Patient
{
    public string PatientUID { get; set; }
    public User PatientUser { get; set; }
    public List<FavoriteDoctors> FavoriteDoctors { get; set; }
    public List<FavoriteClinics> FavoriteClinics { get; set; }

    public List<PatientLocation> Locations { get; set; }
    public List<Appointment> Appointments { get; set; }
}