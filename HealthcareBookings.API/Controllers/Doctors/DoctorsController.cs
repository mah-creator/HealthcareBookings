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
using Microsoft.EntityFrameworkCore;
using HealthcareBookings.API.Controllers.Clinics;
using HealthcareBookings.Domain.Exceptions;

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
	[ProducesResponseType(typeof(PagedList<DoctorDto>), 200)]
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

	[HttpGet("clinic/{clinicId}")]
	[ProducesResponseType(typeof(PagedList<DoctorDto>), 200)]
	public async Task<IActionResult> GetDoctorsByClinic(string clinicId, string? categoryId, GetDoctorsQuery query)
	{
		var doctors = mediator.Send(query).Result
			.Where(d => d.ClinicID == clinicId);
		if (!string.IsNullOrEmpty(categoryId) && !string.IsNullOrWhiteSpace(categoryId))
			doctors = doctors.Where(d => d.CategoryID == categoryId);

		foreach (var item in doctors)
		{
			Console.WriteLine(item);
		}

		var doctorDtos = doctors.Select(d => CreateDoctorDto(d, patient));

		foreach (var item in doctorDtos)
		{
			Console.WriteLine(item);
		}

		return Ok();
		//return
		//	Ok(
		//		PagedList<DoctorDto>
		//		.CreatePagedList(doctorDtos, query.Page, query.PageSize)
		//	);
	}

	[HttpGet("{categoryId}")]
	[ProducesResponseType(typeof(PagedList<DoctorDto>), 200)]
	public async Task<IActionResult> GetDoctorsByCategory(string categoryId, GetDoctorsQuery query)
	{
		var doctors = await mediator.Send(query);
		var doctorDtosByCategory = doctors
			.Where(d => d.CategoryID == categoryId)
			.Select(d => CreateDoctorDto(d, patient));

		return Ok(
				PagedList<DoctorDto>
				.CreatePagedList(doctorDtosByCategory, query.Page, query.PageSize));
	}

	[HttpGet("one")]
	[ProducesResponseType(typeof(DoctorDto), 200)]
	[ProducesResponseType(typeof(string), 400)]
	public async Task<IActionResult> GetDoctorById(string doctorId)
	{
		var patient = await currentUserEntityService.GetCurrentPatient();
		var doctor = dbContext.Doctors.Include(d => d.Appointments).ThenInclude(a => a.Review).FirstOrDefault(d => d.DoctorUID == doctorId);
		if (doctor == null)
			throw new InvalidHttpActionException("Doctor wasn't found");

		return Ok(CreateDoctorDto(doctor, patient));
	}
}