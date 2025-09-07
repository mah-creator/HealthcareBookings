using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks.Dataflow;

namespace HealthcareBookings.API.Controllers.Clinics;

[Controller]
[Route("/api/clinics")]
[Authorize(Roles = UserRoles.Patient)]
public class ClinicsController(
	IAppDbContext dbContext,
	CurrentUserEntityService currentUserEntityService) : ControllerBase
{
	private User patient = currentUserEntityService.GetCurrentPatient().Result;

	[HttpGet]
	[ProducesResponseType(typeof(List<ClinicDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public IActionResult GetClinics()
	{
		return dbContext.Clinics.Any() ?
			Ok(dbContext.Clinics
				.AsNoTracking()
				.Select(c => CreateClinicDto(c, patient)))
			: BadRequest();
	}

	[HttpGet("{id}")]
	[ProducesResponseType(typeof(ClinicDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public IActionResult GetClinicById(string id)
	{
		if (!dbContext.Clinics.Any()) 
			return BadRequest();

		var clinic = dbContext.Clinics.Find(id);

		return clinic != null ? 
			Ok(CreateClinicDto(clinic, patient))
			: NotFound();
	}

	[HttpGet("nearby")]
	[ProducesResponseType(typeof(List<ClinicDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public IActionResult GetNearbyClinics([Required]float longitude, [Required]float latitude)
	{
		if (!dbContext.Clinics.Any())
			return BadRequest();

		var clinics = dbContext.Clinics
			.Where(c => Math.Abs(c.Location.Longitude - longitude) <= 20 
					 && Math.Abs(c.Location.Latitude - latitude) <= 10)
			.AsNoTracking()
			.Select(c => CreateClinicDto(c, patient));

		return clinics.Count() > 0 ?  Ok(clinics) : NotFound();
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