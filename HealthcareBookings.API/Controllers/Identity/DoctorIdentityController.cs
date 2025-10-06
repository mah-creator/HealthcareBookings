using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Doctors.Commands;
using HealthcareBookings.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareBookings.API.Controllers.Identity;

[Controller]
[Route("/api/identity")]
[Tags("Identity")]
[Authorize(Roles = UserRoles.ClinicAdmin)]

public class DoctorIdentityController(IMediator mediator, IAppDbContext dbContext) : ControllerBase
{
	[HttpPost("registerDoctor")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorCommand command)
	{
		await mediator.Send(command);
		return NoContent();
	}
}
