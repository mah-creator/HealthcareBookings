using HealthcareBookings.Domain.Entities;

namespace HealthcareBookings.Application.Clinics.Commands.Profile;
public class ClinicProfileDto
{
	public string Name { get; set; }
	public string Description { get; set; }
	public string ProfileImagePath { get; set; }
	public Location Location { get; set; }
}