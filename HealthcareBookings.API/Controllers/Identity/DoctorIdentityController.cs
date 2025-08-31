using HealthcareBookings.Application.Doctor.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareBookings.API.Controllers.Identity;

[Controller]
[Route("/api/identity/doctor")]
public class DoctorIdentityController(IMediator mediator) : ControllerBase
{
	[HttpPost("register")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> RegisterPatient([FromBody] RegisterDoctorCommand command)
	{
		await mediator.Send(command);
		return NoContent();
	}
}
