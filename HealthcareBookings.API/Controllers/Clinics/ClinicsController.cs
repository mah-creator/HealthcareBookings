using HealthcareBookings.Application.Clinics.Queries;
using HealthcareBookings.Application.Constants;
using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Paging;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using HealthcareBookings.Application.Clinics;
using static HealthcareBookings.Application.Clinics.Utils;

namespace HealthcareBookings.API.Controllers.Clinics;

[Controller]
[Route("/api/clinics")]
[Authorize(Roles = UserRoles.Patient)]
public class ClinicsController(
	IMediator mediator,
	IAppDbContext dbContext,
	CurrentUserEntityService currentUserEntityService) : ControllerBase
{
	private User patient = currentUserEntityService.GetCurrentPatient().Result;

	[HttpGet]
	[ProducesResponseType(typeof(PagedList<ClinicDto>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetClinics(GetClinicsQuery query)
	{
		var clinicsQuery = await mediator.Send(query);
		var clinicsDtoQuery = clinicsQuery
			.Select(c => CreateClinicDto(c, patient, null, null));

		return 
			Ok(
				PagedList<ClinicDto>
				.CreatePagedList(clinicsDtoQuery, query.Page, query.PageSize)
			);
	}

	[HttpGet("{id}")]
	[ProducesResponseType(typeof(ClinicDto), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(string), 400)]
	public IActionResult GetClinicById(string id)
	{
		var clinic = dbContext.Clinics?.Find(id);

		return clinic != null ? 
			Ok(CreateClinicDto(clinic, patient, null, null))
			: throw new InvalidHttpActionException("Clinic wasn't found");
	}

	[HttpGet("nearby")]
	[ProducesResponseType(typeof(PagedList<ClinicDto>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetNearbyClinics([Required]float longitude, [Required]float latitude, GetClinicsQuery query)
	{
		var clinicsQuery = await mediator.Send(query);
		var clinicDtosQuery = clinicsQuery
			.AsNoTracking()
			.Select(c => CreateClinicDto(c, patient, latitude, longitude)).AsEnumerable();
		var nearby = clinicDtosQuery.Where(c => c.DestanceKm != null && c.DestanceKm <= 3.0);

		return 
			Ok(
				PagedList<ClinicDto>
				.CreatePagedList(nearby.AsQueryable(), query.Page, query.PageSize)
			);
	}
}