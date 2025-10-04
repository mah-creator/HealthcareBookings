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
using static HealthcareBookings.Application.Utils.GeoUtils;

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
	private static ClinicDto CreateClinicDto(Clinic c, User patient, float? latitude, float? longitude)
	{
		var distance = DistanceKm(c.Location.Latitude, c.Location.Longitude, ((double?)latitude), ((double?)longitude));
		return new ClinicDto
		{
			Id = c.ClinicID,
			Name = c.ClinicName,
			Image = ApiSettings.BaseUrl + c.ImagePath,
			Description = c.ClinicDescription!,
			Rating = c.Rating,
			RatingCount = c.RatingCount,
			Location = c.Location,
			IsFavorite = patient.PatientProperties.FavoriteClinics.Find(fc => fc.ClinicID == c.ClinicID) is not null,
			DestanceKm = distance,
			Distance = distance == null ? null : 
				distance >= 1 ? string.Format("{0:F1} Km", distance) : string.Format("{0:D} m", (int?)(distance * 1000))
		};
	}
}

internal struct ClinicDto
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string Image {  get; set; }
	public string Description { get; set; }
	public float Rating { get; set; }
	public int RatingCount { get; set; }
	public bool IsFavorite { get; set; }
	public double? DestanceKm { get; set; }
	public string? Distance {  get; set; }
	public Location Location { get; set; }
}