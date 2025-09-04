using HealthcareBookings.Application.Data;
using HealthcareBookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HealthcareBookings.Application.Users;

public class CurrentUserEntityService(CurrentUserService currentUserService,
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
			.Include(u => u.PatientProperties)
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
