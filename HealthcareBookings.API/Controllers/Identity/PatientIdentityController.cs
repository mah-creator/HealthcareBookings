using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Patients.Commands;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace HealthcareBookings.API.Controllers.Identity;

[Controller]
[Route("/api/identity")]
[Tags("Identity")]
public class PatientIdentityController(
	UserRegistrationService userRegistrationService,
	IAppDbContext dbContext) : ControllerBase
{
    [HttpPost("registerPatient")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> RegisterPatient([FromBody] RegisterRequest request)
    {
		var newUser = await userRegistrationService.RegisterUser(
			 email: request.Email,
			 password: request.Password,
			 role: UserRoles.Patient);

		newUser.PatientProperties = new Patient { PatientUID = newUser.Id };

		await dbContext.SaveChangesAsync();
		return NoContent();
    }
}
