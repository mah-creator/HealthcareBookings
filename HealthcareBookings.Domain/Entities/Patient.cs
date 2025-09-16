namespace HealthcareBookings.Domain.Entities;

public class Patient
{
    public string PatientUID { get; set; }
    public User Account { get; set; }
    public List<FavoriteDoctor> FavoriteDoctors { get; set; }
    public List<FavoriteClinic> FavoriteClinics { get; set; }

    public List<PatientLocation> Locations { get; set; }
    public List<Appointment> Appointments { get; set; }
}