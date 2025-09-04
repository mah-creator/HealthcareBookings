namespace HealthcareBookings.Application.Patients.Queries;

public class GetPatientProfileQuery
{
	public string Name { get; set; }
	public DateOnly DateOfBirth { get; set; }
	public string Gender { get; set; }
	public string ProfileImagePath { get; set; }
}
