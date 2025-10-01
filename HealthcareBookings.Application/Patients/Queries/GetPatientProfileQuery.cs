using HealthcareBookings.Application.Patients.Queries;

namespace HealthcareBookings.Application.Patients.Queries;

public class GetPatientProfileQuery
{
	public string Name { get; set; }
	public DateOnly DateOfBirth { get; set; }
	public string Gender { get; set; }
	public  IEnumerable<LocationDto> Locations { get; set; }
	public string ProfileImagePath { get; set; }
}


public struct LocationDto
{
	public string? LocationName { get; set; }
	public float? Longitude { get; set; }
	public float? Latitude { get; set; }
	public bool? IsPrimary { get; set; }
}