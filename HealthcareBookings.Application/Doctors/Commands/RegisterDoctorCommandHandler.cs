using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBookings.Application.Doctors.Commands;

public class RegisterDoctorCommandHandler(
	UserRegistrationService userRegistrationService,
	CurrentUserService currentUserService,
	IAppDbContext dbContext) : IRequestHandler<RegisterDoctorCommand>
{
	/*
	 * doctor is created only after validating the clinic profile is created
	 * also, verifying the doctor category
	 */
	public async Task Handle(RegisterDoctorCommand request, CancellationToken cancellationToken)
	{
		var currentUser = currentUserService.GetCurrentUser();
		var clinic = dbContext.ClinicAdmins
			.Include(ca => ca.Clinic)
			.Where(ca => ca.ClinicAdminUID == currentUser.Id)
			.FirstOrDefault()
			?.Clinic;

		var category = dbContext.DoctorCategories
			.Where(dc => dc.NormalizedName == request.Category.ToUpper())
			.FirstOrDefault();

		if (clinic == null)
		{
			throw new InvalidHttpActionException("Clinic profile isn't setup");
		}

		if (category == null)
		{
			throw new InvalidHttpActionException($"'{request.Category}' isn't a valid category");
		}

		var newUser = await userRegistrationService.RegisterUser(
			email: request.Email,
			password: request.Password,
			role: UserRoles.Doctor,
			cancellationToken);

		newUser.DoctorProperties = new Doctor
		{
			DoctorUID = newUser.Id,
			CategoryID = category!.CategoryID,
			ClinicID = clinic!.ClinicID,
		};

		await dbContext.SaveChangesAsync(cancellationToken);
	}

	internal struct start_end
	{
		public TimeOnly start { get; set; }
		public TimeOnly end { get; set; }
	}
}