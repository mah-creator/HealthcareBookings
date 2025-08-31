using HealthcareBookings.Application.Patient.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareBookings.API.Controllers.Profile;

[Controller]
[Route("/api/profile/patient")]
public class PatientProfileController(IMediator mediator) : ControllerBase
{
	[HttpPost]
	public async Task<IActionResult> CreatePatientProfile(CreatePatientProfileCommand command)
	{
		await mediator.Send(command);
		return NoContent();
	}
}
