using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Doctors.Commands.Profile;
using HealthcareBookings.Application.Doctors.Queries;
using HealthcareBookings.Application.Patients.Queries;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBookings.API.Controllers.Profile;

[Controller]
[Route("api/profile/doctor")]
[Authorize(Roles = $"{UserRoles.Doctor}")]
public class DoctorProfileController(
	IMediator mediator,
	CurrentUserEntityService currentUserEntityService,
	IAppDbContext dbContext) : ControllerBase
{

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> CreateDoctorProfile(CreateDoctorProfileCommand command)
	{
		await mediator.Send(command);
		return NoContent();
	}

	[HttpGet]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(GetDoctorProfileQuery), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetDoctorProfile()
	{
		var user = await currentUserEntityService.GetCurrentDoctor();

		if (user.Profile is null || user.DoctorProperties is null)
		{
			return BadRequest(new ProblemDetails() { Title = "The doctor has no profile" });
		}

		return Ok(new GetDoctorProfileQuery
		{
			Name = user.Profile.Name,
			DateOfBirth = user.Profile.DOB,
			Gender = user.Profile.Gender,
			ProfileImagePath = user.Profile.ProfileImagePath,
			Category = user.DoctorProperties.Category?.CategoryName,
			Bio = user.DoctorProperties.Bio!
		});
	}

	[HttpPatch]
	public async Task<IActionResult> UpdateDoctorProfile(UpdateDoctorProfileCommand command)
	{
		await mediator.Send(command);
		return NoContent();
	}
}
