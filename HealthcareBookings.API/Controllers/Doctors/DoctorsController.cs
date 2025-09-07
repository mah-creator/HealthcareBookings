using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace HealthcareBookings.API.Controllers.Doctors;

[Controller]
[Route("/api/doctors")]
[Authorize(Roles = UserRoles.Patient)]
public class DoctorsController : ControllerBase
{
	public User patient;
	private IAppDbContext _dbContext;

	public DoctorsController(
	IAppDbContext dbContext,
	CurrentUserEntityService currentUserEntityService)
	{
		_dbContext = dbContext;
		patient = currentUserEntityService.GetCurrentPatient().Result;
	}

	[HttpGet]
	[ProducesResponseType(typeof(List<DoctorDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public IActionResult GetDoctors()
	{
		var doctors = _dbContext.Doctors.IncludeAll();

		return doctors.Any() ? 
			Ok(doctors.Select(d => CreateDoctorDto(d, patient)))
			: BadRequest();
	}

	[HttpGet("{categoryId}")]
	[ProducesResponseType(typeof(List<DoctorDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public IActionResult GetDoctorsByCategory(string categoryId)
	{
		var doctors = _dbContext.Doctors.IncludeAll();
		if (!doctors.Any())
			return BadRequest();

		var doctorsByCategory = doctors
			.Where(d => d.CategoryID.Equals(categoryId));

		return doctorsByCategory.Any() ?
			Ok(doctorsByCategory.Select(d => CreateDoctorDto(d, patient)))
			: NotFound();
	}

	private static DoctorDto CreateDoctorDto(Doctor d, User patient)
	{
		return new DoctorDto
		{
			Id = d.DoctorUID,
			Name = d.Account.Profile.Name,
			IsFavorite = patient.PatientProperties?.FavoriteDoctors?.Find(fd => fd.DoctorID == d.DoctorUID) is not null,
			ClinicName = d.Clinic.ClinicName,
			ClinicLocation = d.Clinic.Location.ToString(),
			Rating = d.Appointments.Sum(a => a.Review.Rating) / d.Appointments.Count(a => a.Review != null),
			Reviews = d.Appointments.Count(a => a.Review != null)
		};
	}
}

internal static class DoctorsDbSetIncludablesExtention
{
	public static IQueryable<Doctor> IncludeAll(this IQueryable<Doctor> doctors) 
	{
		return doctors
			.Include(d => d.Category)
			.Include(d => d.Account).ThenInclude(a => a.Profile)
			.Include(d => d.Clinic).ThenInclude(c => c.Location)
			.Include(d => d.Appointments).ThenInclude(a => a.Review);
	}

}

internal struct DoctorDto
{
	public string Id { get; set; }
	public string Name { get; set; }
	public bool IsFavorite { get; set; }
	public string ClinicName { get; set; }
	public string ClinicLocation { get; set; }
	public float Rating { get; set; }
	public int Reviews { get; set; }
}