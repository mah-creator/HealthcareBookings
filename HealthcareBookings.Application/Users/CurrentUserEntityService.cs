using HealthcareBookings.Application.Data;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HealthcareBookings.Application.Users;

public class CurrentUserEntityService(
	CurrentUserService currentUserService,
	UserManager<User> userManager,
	IAppDbContext dbContext)
{
	public async Task<User> GetCurrentDoctor()
	{
		var currentUser = currentUserService.GetCurrentUser();
		var currentDoctor = dbContext.Users
			.Include(u => u.DoctorProperties)
			.ThenInclude(doc => doc.Category)
			.Include(u => u.Profile)
			.Where(u => u.Id == currentUser.Id)
			.First();

		return currentDoctor;
	}

	public async Task<User> GetCurrentPatient()
	{
		var currentUser = currentUserService.GetCurrentUser();
		var currentDoctor = dbContext.Users
			.Include(u => u.PatientProperties).ThenInclude(p => p.FavoriteDoctors)
			.Include(u => u.PatientProperties).ThenInclude(p => p.FavoriteClinics)
			.Include(u => u.PatientProperties).ThenInclude(p => p.Appointments)
				.ThenInclude(a => a.Doctor).ThenInclude(d => d.Clinic).ThenInclude(c => c.Location)
			.Include(u => u.PatientProperties).ThenInclude(p => p.Appointments)
				.ThenInclude(a => a.Doctor).ThenInclude(d => d.Account).ThenInclude(d => d.Profile)
			.Include(u => u.PatientProperties).ThenInclude(p => p.Appointments)
				.ThenInclude(a => a.Doctor).ThenInclude(d => d.Category)
			.Include(u => u.PatientProperties).ThenInclude(p => p.Appointments)
				.ThenInclude(a => a.TimeSlot).ThenInclude(ts => ts.Schedule)
			.Include(u => u.Profile)
			.Where(u => u.Id == currentUser.Id)
			.First();

		return currentDoctor;
	}

	public async Task<User> GetCurrentClinicAdmin()
	{
		var currentUser = currentUserService.GetCurrentUser();
		var currentDoctor = dbContext.Users
			.Include(u => u.Profile)
			.Include(u => u.ClinicAdminProperties)
			.ThenInclude(ca => ca.Clinic)
			.Where(u => u.Id == currentUser.Id)
			.First();

		return currentDoctor;
	}
}
