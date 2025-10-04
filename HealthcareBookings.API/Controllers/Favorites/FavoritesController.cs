using HealthcareBookings.API.Controllers.Clinics;
using HealthcareBookings.Application.Constants;
using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Paging;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthcareBookings.Application.Clinics;
using static HealthcareBookings.Application.Clinics.Utils;
using static HealthcareBookings.Application.Doctors.Utils;
using HealthcareBookings.Application.Doctors.Queries;

namespace HealthcareBookings.API.Controllers.Favorites;

[Controller]
[Route("/api/favorites")]
[Authorize(Roles = UserRoles.Patient)]
public class FavoritesController(
	IAppDbContext dbContext,
	CurrentUserEntityService currentUserEntityService) : ControllerBase
{
	[HttpPost("clinics")]
	[ProducesResponseType(200)]
	[ProducesResponseType(typeof(string), 400)]
	public async Task<IActionResult> AddClinicToFavorites(string clinicId)
	{
		var clinic = dbContext.Clinics.Find(clinicId);
		if (clinic == null)
		{
			throw new InvalidHttpActionException("Clinic wasn't found");
		}

		var patient = await currentUserEntityService.GetCurrentPatient();
		if (patient.PatientProperties.FavoriteClinics.Find(fc => fc.ClinicID == clinic.ClinicID) != null)
		{
			throw new InvalidHttpActionException($"You already added '{clinic.ClinicName}' to your favotites");
		}

		patient.PatientProperties.FavoriteClinics.Add(new()
		{
			ClinicID = clinic.ClinicID,
			PatientID = patient.Id
		});

		await dbContext.SaveChangesAsync();

		return Ok($"'{clinic.ClinicName}' was added to your favorites");
	}

	[HttpGet("clinics")]
	[ProducesResponseType(typeof(PagedList<ClinicDto>), 200)]
	public async Task<IActionResult> GetFavoriteClinics(int page, int pageSize)
	{
		var patient = await currentUserEntityService.GetCurrentPatient();
		var favoriteClinics = patient.PatientProperties?.FavoriteClinics?.Select(c => CreateClinicDto(c.Clinic, patient, null, null)).AsQueryable();

		return Ok(PagedList<ClinicDto>.CreatePagedList(favoriteClinics, page, pageSize));
	}

	[HttpDelete("clinics")]
	[ProducesResponseType(200)]
	[ProducesResponseType(typeof(string), 400)]
	public async Task<IActionResult> RemoveFavoriteClinic(string clinicId)
	{
		var patient = await currentUserEntityService.GetCurrentPatient();
		var clinic = patient.PatientProperties.FavoriteClinics.Where(c => c.ClinicID == clinicId).FirstOrDefault();
		
		if (clinic == null)
		{
			throw new InvalidHttpActionException("Clinic wasn't found");
		}

		patient.PatientProperties.FavoriteClinics.Remove(clinic);
		await dbContext.SaveChangesAsync();

		return Ok($"Clinic {clinic.Clinic.ClinicName} was removed from favorites");
	}

	[HttpPost("doctors")]
	[ProducesResponseType(200)]
	[ProducesResponseType(typeof(string), 400)]
	public async Task<IActionResult> AddDoctorToFavorites(string doctorId)
	{
		var doctor = dbContext.Doctors
			.Include(d => d.Account).ThenInclude(a => a.Profile)
			.Where(d => d.DoctorUID == doctorId).FirstOrDefault();
		if (doctor == null)
		{
			throw new InvalidHttpActionException("Doctor wasn't found");
		}

		var patient = await currentUserEntityService.GetCurrentPatient();
		if (patient.PatientProperties.FavoriteDoctors.Find(fd => fd.DoctorID == doctor.DoctorUID) != null)
		{
			throw new InvalidHttpActionException($"You've already added '{doctor.Account.Profile.Name}' to your favorites");
		}

		patient.PatientProperties.FavoriteDoctors.Add(new()
		{
			DoctorID = doctor.DoctorUID,
			PatientID = patient.Id
		});

		await dbContext.SaveChangesAsync();

		return Ok($"Doctor '{doctor.Account.Profile.Name}' was added to your favorites");
	}

	[HttpGet("doctors")]
	[ProducesResponseType(typeof(PagedList<DoctorDto>), 200)]
	public async Task<IActionResult> GetFavoriteDoctors(int page, int pageSize)
	{
		var patient = await currentUserEntityService.GetCurrentPatient();
		var favoriteDoctors = patient.PatientProperties?.FavoriteDoctors?.Select(d => CreateDoctorDto(d.Doctor, patient))
		.AsQueryable(); ;

		return Ok(PagedList<DoctorDto>.CreatePagedList(favoriteDoctors, page, pageSize));
	}

	[HttpDelete("doctors")]
	[ProducesResponseType(200)]
	[ProducesResponseType(typeof(string), 200)]
	public async Task<IActionResult> RemoveFavoriteDoctor(string doctorId)
	{
		var patient = await currentUserEntityService.GetCurrentPatient();
		var doctor = patient.PatientProperties.FavoriteDoctors.Where(d => d.DoctorID == doctorId).FirstOrDefault();

		if (doctor == null)
		{
			throw new InvalidHttpActionException("Doctor wasn't found");
		}

		patient.PatientProperties.FavoriteDoctors.Remove(doctor);
		await dbContext.SaveChangesAsync();

		return Ok($"Doctor '{doctor.Doctor.Account.Profile.Name}' removed from favorites");
	}
}