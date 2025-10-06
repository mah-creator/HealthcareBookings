using HealthcareBookings.Application.Constants;
using HealthcareBookings.Application.Patients.Commands.Profile;
using HealthcareBookings.Application.Patients.Queries;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Application.Validators;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Domain.Exceptions;
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
		var patient = currentUserEntityService.GetCurrentPatient().Result.PatientProperties;
		return Ok(createPatientProfileDto(profile));

	}

	[HttpGet]
	[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(GetPatientProfileQuery), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetPatientProfile()
	{
		var profile = currentUserEntityService.GetCurrentPatient().Result.Profile;
		var patient = currentUserEntityService.GetCurrentPatient().Result.PatientProperties;

		if (profile is null || patient is null)
		{
			throw new InvalidHttpActionException("Your patient profile wasn't set up correctly");
		}

		return Ok(createPatientProfileDto(profile));
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

		return Ok(createPatientProfileDto(profile));
	}
	private GetPatientProfileQuery createPatientProfileDto(ProfileInformation profile)
	{
		return new GetPatientProfileQuery
		{
			Name = profile.Name ?? UserRoles.Patient,
			DateOfBirth = profile.DOB,
			Gender = profile.Gender,
			ProfileImagePath = ApiSettings.BaseUrl + profile.ProfileImagePath
		};
	}
}
