using HealthcareBookings.Application.Doctors.Queries;
using HealthcareBookings.Application.Paging;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareBookings.API.Controllers.Clinics;

[Controller]
[Route("/api/clinic-doctors")]
[Authorize(Roles = UserRoles.ClinicAdmin)]
public class ClinicDoctors(IMediator mediator, CurrentUserEntityService currentUserEntityService) : ControllerBase
{
	[HttpGet]
	public async Task<IActionResult> GetClinicDoctors(GetDoctorsQuery query)
	{
		var clinic = currentUserEntityService.GetCurrentClinicAdmin().Result.ClinicAdminProperties?.Clinic;

		if (clinic == null)
			return BadRequest("Clinic wasn't found");

		var doctors = await mediator.Send(query);
		doctors = doctors?.Where(d => d.ClinicID == clinic.ClinicID);
		var doctorDtos = doctors?.AsEnumerable().Select(d => new DoctorDto
		{
			Id = d.DoctorUID,
			Name = d.Account?.Profile?.Name,
			Rating = d.Rating,
			Reviews = d.Appointments?.Count(a => a.Review != null) ?? 0,
			Experience = d.ExperienceYears,
			Bio = d.Bio,
			PatientCount = d.Appointments?.DistinctBy(a => a.PatientID).Count() ?? 0
		}).AsQueryable();

		return
			Ok(
				PagedList<DoctorDto>
				.CreatePagedList(doctorDtos, query.Page, query.PageSize)
			);
	}
}

internal struct DoctorDto
{
	public string Id { get; set; }
	public string Name { get; set; }
	public float Rating { get; set; }
	public int Reviews { get; set; }
	public int Experience { get; set; }
	public int PatientCount { get; set; }
	public string Bio { get; set; }
}
