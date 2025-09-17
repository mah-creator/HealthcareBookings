using HealthcareBookings.Application.Clinics.Commands.Profile;
using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.StaticFiles.Uploads;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Application.Validators;
using HealthcareBookings.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace HealthcareBookings.API.Controllers.Profile;

[Controller]
[Route("api/profile/clinic")]
[Authorize(Roles = UserRoles.ClinicAdmin)]
public class ClinicProfileController(
	IMediator mediator,
	CurrentUserEntityService currentUserEntityService,
	FileUploadService fileUploadService,
	IAppDbContext dbContext) : ControllerBase
{
	[HttpPost]
	[ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ClinicProfileDto), StatusCodes.Status200OK)]
	public async Task<IActionResult> CreateClinicProfile(
		[FromBody] CreateClinicProfileCommand command,
		[FromServices] CreateClinicProfileComaandValidator validator)
	{
		var validationResult = await validator.ValidateAsync(command);
		if (!validationResult.IsValid)
		{
			return BadRequest(new HttpValidationProblemDetails(validationResult.ToDictionary()));
		}
		
		var clinicDto = await mediator.Send(command);

		return Ok(clinicDto);
	}

	[HttpPatch("profileimage")]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> UpdateClinicProfileImage(IFormFile image)
	{
		var clinicAdmin = await currentUserEntityService.GetCurrentClinicAdmin();
		var clinic = clinicAdmin.ClinicAdminProperties?.Clinic;
		
		if (clinic is null)
		{
			return BadRequest(new ProblemDetails() { Title = "Clinic profile wasn't created" });
		}

		var imagePath = await fileUploadService.UploadWebAsset(image);

		clinic.ImagePath = imagePath;

		await dbContext.SaveChangesAsync();

		return Ok(new { profileImagePath = imagePath });
	}

	[HttpGet]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ClinicProfileDto), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetClinicProfile()
	{
		var  clinicAdmin = await currentUserEntityService.GetCurrentClinicAdmin();
		var clinic = clinicAdmin.ClinicAdminProperties?.Clinic;

		if (clinic is null)
		{
			return BadRequest(new ProblemDetails() { Title = "Clinic profile wasn't created" });
		}

		return Ok(new ClinicProfileDto
		{
			Name = clinic.ClinicName,
			Description = clinic.ClinicDescription!,
			ProfileImagePath = clinic.ImagePath,
			Location = clinic.Location,
		});
	}

	[HttpPatch]
	[ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ClinicProfileDto), StatusCodes.Status200OK)]
	public async Task<IActionResult> UpdateClinicProfile(
		[FromBody] UpdateClinicProfileCommand command,
		[FromServices] UpdateClinicProfileComaandValidator validator)
	{
		var validationResult = await validator.ValidateAsync(command);
		if (!validationResult.IsValid)
		{
			return BadRequest(new HttpValidationProblemDetails(validationResult.ToDictionary()));
		}
		
		var clinicDto = await mediator.Send(command);
		
		return Ok(clinicDto);
	}
}
