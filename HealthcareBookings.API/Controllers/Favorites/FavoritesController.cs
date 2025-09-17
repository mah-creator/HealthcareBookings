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
	public async Task<IActionResult> GetFavoriteClinics(int page, int pageSize)
	{
		var patient = await currentUserEntityService.GetCurrentPatient();
		var favoriteClinics = patient.PatientProperties?.FavoriteClinics?.Select(c => new ClinicDto
		{
			Id = c.ClinicID,
			Name = c.Clinic.ClinicName,
			ClinicImagePath = c.Clinic.ImagePath,
			Address = c.Clinic.Location.ToString()
		}).AsQueryable();

		return Ok(PagedList<ClinicDto>.CreatePagedList(favoriteClinics, page, pageSize));
	}

	[HttpDelete("clinics")]
	public async Task<IActionResult> RemoveFavoriteClinic(string clinicId)
	{
		var patient = await currentUserEntityService.GetCurrentPatient();
		var clinic = patient.PatientProperties.FavoriteClinics.Where(c => c.ClinicID == clinicId).FirstOrDefault();
		
		if (clinic == null)
		{
			return BadRequest("Clinic wasn't found");
		}

		patient.PatientProperties.FavoriteClinics.Remove(clinic);
		await dbContext.SaveChangesAsync();

		return Ok();
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
			DoctorID = doctor.DoctorUID,
			PatientID = patient.Id
		});

		await dbContext.SaveChangesAsync();

		return Ok();
	}

	[HttpGet("doctors")]
	public async Task<IActionResult> GetFavoriteDoctors(int page, int pageSize)
	{
		var patient = await currentUserEntityService.GetCurrentPatient();
		var favoriteDoctors = patient.PatientProperties?.FavoriteDoctors?.AsQueryable().Select(d => new DoctorDto
		{
			Id = d.DoctorID,
			Name = d.Doctor.Account.Profile.Name,
			DoctorImagePath = d.Doctor.Account.Profile.ProfileImagePath,
			ClinicName = d.Doctor.Clinic.ClinicName,
			ClinicCity = d.Doctor.Clinic.Location.City,
			Rating = d.Doctor.Rating,
			Reviews = dbContext.Appointments.Where(a => a.DoctorID == d.DoctorID && a.Review != null).Count()
		}).AsQueryable(); ;

		return Ok(PagedList<DoctorDto>.CreatePagedList(favoriteDoctors, page, pageSize));
	}

	[HttpDelete("doctors")]
	public async Task<IActionResult> RemoveFavoriteDoctor(string doctorId)
	{
		var patient = await currentUserEntityService.GetCurrentPatient();
		var doctor = patient.PatientProperties.FavoriteDoctors.Where(d => d.DoctorID == doctorId).FirstOrDefault();

		if (doctor == null)
		{
			return BadRequest("Clinic wasn't found");
		}

		patient.PatientProperties.FavoriteDoctors.Remove(doctor);
		await dbContext.SaveChangesAsync();

		return Ok();
	}
}

internal class ClinicDto
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string Address { get; set; }
	public string ClinicImagePath { get; set; }
}

internal class DoctorDto
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string DoctorImagePath { get; set; }
	public string ClinicName { get; set; }
	public string ClinicCity { get; set; }
	public float Rating { get; set; }
	public int Reviews { get; set; }
}