using HealthcareBookings.Application.Patient.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareBookings.API.Controllers.Identity.Patient;

[Controller]
[Route("/api/identity/patient")]
public class PatientIdentityController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientCommand command)
    {
	   await mediator.Send(command);
	   return NoContent();
    }
}
