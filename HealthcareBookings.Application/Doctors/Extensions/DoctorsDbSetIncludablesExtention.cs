using HealthcareBookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBookings.Application.Doctors.Extensions;

public static class DoctorsDbSetIncludablesExtention
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