using HealthcareBookings.Application.Clinics.Commands.Profile;
using HealthcareBookings.Application.Clinics.Queries;
using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Patients.Queries;
using HealthcareBookings.Application.StaticFiles.Uploads;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> CreateClinicProfile([FromBody] CreateClinicProfileCommand command)
	{
		await mediator.Send(command);
		return NoContent();
	}

	[HttpPatch("profileimage")]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
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

		return NoContent();
	}

	[HttpGet]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(GetClinicProfileQuery), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetClinicProfile()
	{
		var  clinicAdmin = await currentUserEntityService.GetCurrentClinicAdmin();
		var clinic = clinicAdmin.ClinicAdminProperties?.Clinic;

		if (clinic is null)
		{
			return BadRequest(new ProblemDetails() { Title = "Clinic profile wasn't created" });
		}


		return Ok(new GetClinicProfileQuery
		{
			Name = clinic.ClinicName,
			Description = clinic.ClinicDescription!,
			ProfileImagePath = clinic.ImagePath,
			Location = clinic.Location,
		});
	}

	//[HttpPatch]
	//public async Task<IActionResult> UpdateClinicProfile(UpdateClinicProfileCommand command)
	//{
	//	await mediator.Send(command);
	//	return NoContent();
	//}
}
