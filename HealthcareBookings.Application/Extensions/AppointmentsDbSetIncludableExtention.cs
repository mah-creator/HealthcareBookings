using HealthcareBookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBookings.Application.Extensions;

public static class AppointmentsDbSetIncludableExtention
{
	public static IQueryable<Appointment> IncludeAll(this IQueryable<Appointment> appointments)
	{
		return appointments
			.Include(d => d.TimeSlot).ThenInclude(d => d.Schedule)
			.Include(d => d.Doctor).ThenInclude(d => d.Clinic)
			.Include(d => d.Doctor).ThenInclude(d => d.Category)
			.Include(d => d.Doctor).ThenInclude(d => d.Account).ThenInclude(d => d.Profile);
	}

}