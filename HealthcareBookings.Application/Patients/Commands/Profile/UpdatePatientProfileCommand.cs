using MediatR;
using Microsoft.AspNetCore.Http;

namespace HealthcareBookings.Application.Patients.Commands.Profile;

public class UpdatePatientProfileCommand : IRequest
{
	public string? Name { get; set; }
	public string? Gender { get; set; }
	public DateOnly? DateOfBirth { get; set; }
	public IFormFile? ProfileImage { get; set; }
}
