using HealthcareBookings.Application.Clinics.Queries;
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
			.Select(c => CreateClinicDto(c, patient));

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
			Ok(CreateClinicDto(clinic, patient))
			: throw new InvalidHttpActionException("Clinic wasn't found");
	}

	[HttpGet("nearby")]
	[ProducesResponseType(typeof(PagedList<ClinicDto>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetNearbyClinics([Required]float longitude, [Required]float latitude, GetClinicsQuery query)
	{
		var clinicsQuery = await mediator.Send(query);
		var clinicDtosQuery = clinicsQuery
			.Where(c => Math.Abs(c.Location.Longitude - longitude) <= 20 
					 && Math.Abs(c.Location.Latitude - latitude) <= 10)
			.AsNoTracking()
			.Select(c => CreateClinicDto(c, patient));

		return 
			Ok(
				PagedList<ClinicDto>
				.CreatePagedList(clinicDtosQuery, query.Page, query.PageSize)
			);
	}
	private static ClinicDto CreateClinicDto(Clinic c, User patient)
	{
		return new ClinicDto
		{
			Id = c.ClinicID,
			Name = c.ClinicName,
			Description = c.ClinicDescription!,
			Location = c.Location,
			IsFavorite = patient.PatientProperties.FavoriteClinics.Find(fc => fc.ClinicID == c.ClinicID) is not null
		};
	}
}

internal struct ClinicDto
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public bool IsFavorite { get; set; }
	public Location Location { get; set; }
}