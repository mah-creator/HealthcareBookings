using HealthcareBookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace HealthcareBookings.Application.Patients.Ectensions;

public static class IncludablePatientExtension
{
	public static IQueryable<Patient> IncludeAll(this IQueryable<Patient> patients)
	{
		return patients
			.Include(p => p.Account).ThenInclude(a => a.Profile)
			.Include(p => p.Locations).ThenInclude(l => l.Location);
	}
}
