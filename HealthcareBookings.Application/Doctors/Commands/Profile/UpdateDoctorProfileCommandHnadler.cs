using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Patients.Commands.Profile;
using HealthcareBookings.Application.StaticFiles.Uploads;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;

namespace HealthcareBookings.Application.Doctors.Commands.Profile;

public class UpdateDoctorProfileCommandHnadler(CurrentUserService currentUserService,
	FileUploadService fileUploadService,
	IAppDbContext dbContext) : IRequestHandler<UpdateDoctorProfileCommand>
{
	public async Task Handle(UpdateDoctorProfileCommand request, CancellationToken cancellationToken)
	{
		var currentUser = currentUserService.GetCurrentUser();
		var user = dbContext.Users
			.Include(u => u.Profile)
			.Include(u => u.DoctorProperties)
			.Where(u => u.Id == currentUser.Id)
			.ToList()
			.First();

		if (user.Profile is null)
		{
			throw new InvalidHttpActionException($"User '{user.Email}' has no profile");
		}

		user.Profile.Name = request.Name ?? user.Profile.Name;
		user.Profile.Gender = request.Gender ?? user.Profile.Gender;
		user.Profile.DOB = request.DateOfBirth ?? user.Profile.DOB;
		user.DoctorProperties.Bio = request.Bio ?? user.DoctorProperties.Bio;
		user.DoctorProperties.ExperienceYears = request.ExperienceYears ?? user.DoctorProperties.ExperienceYears;


		if (request.ProfileImage != null)
		{
			var relativePath = await fileUploadService.UploadWebAsset(request.ProfileImage);

			user.Profile.ProfileImagePath = relativePath;

		}
		await dbContext.SaveChangesAsync(cancellationToken);
	}
}
