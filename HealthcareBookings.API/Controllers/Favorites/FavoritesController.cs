using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Paging;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBookings.API.Controllers.Favorites;

[Controller]
[Route("/api/favorites")]
[Authorize(Roles = UserRoles.Patient)]
public class FavoritesController(
	IAppDbContext dbContext,
	CurrentUserEntityService currentUserEntityService) : ControllerBase
{
	[HttpPost("clinics")]
	public async Task<IActionResult> AddClinicToFavorites(string clinicId)
	{
		var clinic = dbContext.Clinics.Find(clinicId);
		if (clinic == null)
		{
			return BadRequest("Clinic wasn't found");
		}

		var patient = await currentUserEntityService.GetCurrentPatient();
		if (patient.PatientProperties.FavoriteClinics.Find(fc => fc.ClinicID == clinic.ClinicID) != null)
		{
			return BadRequest($"You've already favored '{clinic.ClinicName}'");
		}

		patient.PatientProperties.FavoriteClinics.Add(new()
		{
			ClinicID = clinic.ClinicID,
			PatientID = patient.Id
		});

		await dbContext.SaveChangesAsync();

		return Ok();
	}

	[HttpGet("clinics")]
	public async Task<IActionResult> GetFavoriteClinics()
	{
		var patient = await currentUserEntityService.GetCurrentPatient();
		
		return Ok(patient.PatientProperties?.FavoriteClinics);
	}


	[HttpPost("doctors")]
	public async Task<IActionResult> AddDoctorToFavorites(string doctorId)
	{
		var doctor = dbContext.Doctors
			.Include(d => d.Account).ThenInclude(a => a.Profile)
			.Where(d => d.DoctorUID == doctorId).FirstOrDefault();
		if (doctor == null)
		{
			return BadRequest("Doctor wasn't found");
		}

		var patient = await currentUserEntityService.GetCurrentPatient();
		if (patient.PatientProperties.FavoriteDoctors.Find(fd => fd.DoctorID == doctor.DoctorUID) != null)
		{
			return BadRequest($"You've already favored '{doctor.Account.Profile.Name}'");
		}

		patient.PatientProperties.FavoriteDoctors.Add(new()
		{
			DoctorID = doctor.ClinicID,
			PatientID = patient.Id
		});

		await dbContext.SaveChangesAsync();

		return Ok();
	}

	[HttpGet("doctors")]
	public async Task<IActionResult> GetFavoriteDoctors(int page, int pageSize)
	{
		var patient = await currentUserEntityService.GetCurrentPatient();
		var favoriteDoctors = patient.PatientProperties?.FavoriteDoctors.AsQueryable();

		PagedList<FavoriteDoctor>.CreatePagedList(favoriteDoctors, page, pageSize);

		return Ok(patient.PatientProperties?.FavoriteDoctors);
	}
}