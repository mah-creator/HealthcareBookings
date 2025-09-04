using HealthcareBookings.Application.Patients.Commands.Profile;
using HealthcareBookings.Application.Patients.Queries;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBookings.API.Controllers.Profile;

[Controller]
[Route("/api/profile/patient")]
[Authorize(Roles = UserRoles.Patient)]
public class PatientProfileController(IMediator mediator,
	CurrentUserService currentUserService,
	AppDbContext dbContext) : ControllerBase
{
	[HttpPost]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> CreatePatientProfile(CreatePatientProfileCommand command)
	{
		await mediator.Send(command);
		return NoContent();
	}

	[HttpGet]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(GetPatientProfileQuery), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetPatientProfile()
	{
		var currentUser = currentUserService.GetCurrentUser();
		var user = dbContext.Users
			.Include(u => u.Profile)
			.Where(u => u.Id == currentUser.Id).First();

		if (user.Profile is null)
		{
			return BadRequest(new ProblemDetails() { Title = "The user has no profile" });
		}

		return Ok(new GetPatientProfileQuery
		{
			Name = user.Profile.Name,
			DateOfBirth = user.Profile.DOB,
			Gender = user.Profile.Gender,
			ProfileImagePath = user.Profile.ProfileImagePath
		});
	}

	[HttpPatch]
	public async Task<IActionResult> UpdatePatientProfile(UpdatePatientProfileCommand command)
	{
		await mediator.Send(command);
		return NoContent();
	}
}
