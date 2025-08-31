using MediatR;
using Microsoft.AspNetCore.Http;

namespace HealthcareBookings.Application.Patient.Commands;

public class CreatePatientProfileCommand : IRequest
{
	public string Name { get; set; }
	public string Gender { get; set; }
	public DateOnly DateOfBirth { get; set; }
	public IFormFile? FormFile { get; set; }
}