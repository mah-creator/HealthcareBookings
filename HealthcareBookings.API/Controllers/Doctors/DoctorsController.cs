using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Doctors.Queries;
using HealthcareBookings.Application.Doctors.Extensions;
using static HealthcareBookings.Application.Doctors.Utils;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using HealthcareBookings.Application.Paging;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HealthcareBookings.API.Controllers.Doctors;

[Controller]
[Route("/api/doctors")]
[Authorize(Roles = UserRoles.Patient)]
public class DoctorsController(
	IMediator mediator,
	IAppDbContext dbContext,
	CurrentUserEntityService currentUserEntityService) : ControllerBase
{
	private User patient = currentUserEntityService.GetCurrentPatient().Result;

	[HttpGet]
	[ProducesResponseType(typeof(PagedList<DoctorDto>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetDoctors(GetDoctorsQuery query)
	{
		var doctors = await mediator.Send(query);
		var doctorDtos = doctors.Select(d => CreateDoctorDto(d, patient));

		return 
			Ok( 
				PagedList<DoctorDto>
				.CreatePagedList(doctorDtos, query.Page, query.PageSize)
			);
	}

	[HttpGet("{categoryId}")]
	[ProducesResponseType(typeof(PagedList<DoctorDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetDoctorsByCategory(string categoryId, GetDoctorsQuery query)
	{
		var doctors = await mediator.Send(query);
		var doctorDtosByCategory = doctors
			.Where(d => d.CategoryID == categoryId)
			.Select(d => CreateDoctorDto(d, patient));

		return doctorDtosByCategory.Any() ?
			Ok(
				PagedList<DoctorDto>
				.CreatePagedList(doctorDtosByCategory, query.Page, query.PageSize))
			: NotFound();
	}

	[HttpGet("one")]
	public async Task<IActionResult> GetDoctorById(string doctorId)
	{
		var patient = await currentUserEntityService.GetCurrentPatient();
		var doctor = dbContext.Doctors?.Find(doctorId);
		if (doctor == null)
			return BadRequest("Doctor wasn't found");

		return Ok(CreateDoctorDto(doctor, patient));
	}
}