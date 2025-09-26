using HealthcareBookings.Application.Patients.Commands.Profile;
using HealthcareBookings.Application.Patients.Queries;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Application.Validators;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HealthcareBookings.API.Controllers.Profile;

[Controller]
[Route("/api/profile/patient")]
[Authorize(Roles = UserRoles.Patient)]
public class PatientProfileController(IMediator mediator,
	CurrentUserEntityService currentUserEntityService,
	AppDbContext dbContext) : ControllerBase
{
	[HttpPost]
	[ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(GetPatientProfileQuery), StatusCodes.Status200OK)]
	public async Task<IActionResult> CreatePatientProfile(
		CreatePatientProfileCommand command,
		[FromServices] CreatePatientProfileCommandValidator validator)
	{
		var validationResult = await validator.ValidateAsync(command);
		if (!validationResult.IsValid)
		{
			return BadRequest(new HttpValidationProblemDetails(validationResult.ToDictionary()));
		}
		await mediator.Send(command);

		var profile = currentUserEntityService.GetCurrentPatient().Result.Profile;
		return Ok(new GetPatientProfileQuery
		{
			Name = profile.Name,
			DateOfBirth = profile.DOB,
			Gender = profile.Gender,
			ProfileImagePath = profile.ProfileImagePath
		});

	}

	[HttpGet]
	[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(GetPatientProfileQuery), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetPatientProfile()
	{
		var profile = currentUserEntityService.GetCurrentPatient().Result.Profile;

		if (profile is null)
		{
			return BadRequest("The user has no profile");
		}

		return Ok(new GetPatientProfileQuery
		{
			Name = profile.Name,
			DateOfBirth = profile.DOB,
			Gender = profile.Gender,
			ProfileImagePath = profile.ProfileImagePath
		});
	}

	[HttpPatch]
	[ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(GetPatientProfileQuery), StatusCodes.Status200OK)]
	public async Task<IActionResult> UpdatePatientProfile(
		UpdatePatientProfileCommand command,
		[FromServices] UpdatePatientProfileCommandValidator validator)
	{
		var validationResults = await validator.ValidateAsync(command);
		if(!validationResults.IsValid)
		{
			return BadRequest(new HttpValidationProblemDetails(validationResults.ToDictionary()));
		}
		await mediator.Send(command);

		var profile = currentUserEntityService.GetCurrentPatient().Result.Profile;

		return Ok(new GetPatientProfileQuery
		{
			Name = profile.Name,
			DateOfBirth = profile.DOB,
			Gender = profile.Gender,
			ProfileImagePath = profile.ProfileImagePath
		});
	}
}
