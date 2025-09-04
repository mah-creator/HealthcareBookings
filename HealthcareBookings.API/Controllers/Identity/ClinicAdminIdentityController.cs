using HealthcareBookings.Application.Clinics.Commands;
using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace HealthcareBookings.API.Controllers.Identity;

[Controller]
[Route("/api/identity")]
[Tags("Identity")]
public class ClinicAdminIdentityController(
	UserRegistrationService userRegistrationService) : ControllerBase
{
	[HttpPost("registerClinicAdmin")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> RegisterClicinAdmin([FromBody] RegisterRequest request)
	{
		await userRegistrationService.RegisterUser(
			request.Email,
			request.Password,
			UserRoles.ClinicAdmin);

		return NoContent();
	}
}
