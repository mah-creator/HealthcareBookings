namespace HealthcareBookings.Application.Doctors.Queries;

public class GetDoctorProfileQuery
{
	public string Name { get; set; }
	public DateOnly DateOfBirth { get; set; }
	public string Gender { get; set; }
	public string ProfileImagePath { get; set; }
	public string Category { get; set; }
	public string Bio { get; set; }
}
