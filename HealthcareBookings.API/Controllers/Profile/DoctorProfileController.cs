using HealthcareBookings.Application.Doctors.Commands.Profile;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Application.Validators;
using HealthcareBookings.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareBookings.API.Controllers.Profile;

[Controller]
[Route("api/profile/doctor")]
[Authorize(Roles = $"{UserRoles.Doctor}")]
public class DoctorProfileController(
	IMediator mediator,
	CurrentUserEntityService currentUserEntityService) : ControllerBase
{

	[HttpPost]
	[ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(DoctorProfileDto), StatusCodes.Status200OK)]
	public async Task<IActionResult> CreateDoctorProfile(
		CreateDoctorProfileCommand command,
		[FromServices] CreateDoctorProfileCommandValidator validator)
	{
		var validationResult = await validator.ValidateAsync(command);
		if (!validationResult.IsValid)
		{
			return BadRequest(new HttpValidationProblemDetails(validationResult.ToDictionary()));
		}

		await mediator.Send(command);

		var doctor = await currentUserEntityService.GetCurrentDoctor();
		var profile = doctor.Profile;

		return Ok(new DoctorProfileDto
		{
			Name = profile.Name,
			DateOfBirth = profile.DOB,
			Gender = profile.Gender,
			ProfileImagePath = profile.ProfileImagePath,
			Category = doctor.DoctorProperties.Category.CategoryName,
			Bio = doctor.DoctorProperties.Bio!
			ExperienceYears = doctor.DoctorProperties.ExperienceYears,

		});
	}

	[HttpGet]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(DoctorProfileDto), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetDoctorProfile()
	{
		var user = await currentUserEntityService.GetCurrentDoctor();

		if (user.Profile is null || user.DoctorProperties is null)
		{
			return BadRequest(new ProblemDetails() { Title = "The doctor has no profile" });
		}

		return Ok(new DoctorProfileDto
		{
			Name = user.Profile.Name,
			DateOfBirth = user.Profile.DOB,
			Gender = user.Profile.Gender,
			ProfileImagePath = user.Profile.ProfileImagePath,
			Category = user.DoctorProperties.Category.CategoryName,
			Bio = user.DoctorProperties.Bio!,
			ExperienceYears = user.DoctorProperties.ExperienceYears,
		});
	}

	[HttpPatch]
	[ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(DoctorProfileDto), StatusCodes.Status200OK)]
	public async Task<IActionResult> UpdateDoctorProfile(
		UpdateDoctorProfileCommand command,
		[FromServices] UpdateDoctorProfileCommandValidator validator)
	{
		var validationResult = await validator.ValidateAsync(command);
		if (!validationResult.IsValid)
		{
			return BadRequest(new HttpValidationProblemDetails(validationResult.ToDictionary()));
		}
		await mediator.Send(command);

		var doctor = await currentUserEntityService.GetCurrentDoctor();
		var profile = doctor.Profile;

		return Ok(new DoctorProfileDto
		{
			Name = profile.Name,
			DateOfBirth = profile.DOB,
			Gender = profile.Gender,
			ProfileImagePath = profile.ProfileImagePath,
			Category = doctor.DoctorProperties.Category.CategoryName,
			Bio = doctor.DoctorProperties.Bio!,
			ExperienceYears = doctor.DoctorProperties.ExperienceYears,
		});
	}
}

internal record DoctorProfileDto
{
	public string Name { get; set; }
	public DateOnly DateOfBirth { get; set; }
	public string Gender { get; set; }
	public string ProfileImagePath { get; set; }
	public string Category { get; set; }
	public string Bio { get; set; }
	public int ExperienceYears { get; set; }
}
