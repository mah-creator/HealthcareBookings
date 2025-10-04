using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.StaticFiles.Uploads;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;

namespace HealthcareBookings.Application.Patients.Commands.Profile;

public class UpdatePatientProfileCommandHnadler(CurrentUserService currentUserService,
	FileUploadService fileUploadService,
	IAppDbContext context) : IRequestHandler<UpdatePatientProfileCommand>
{
	public async Task Handle(UpdatePatientProfileCommand request, CancellationToken cancellationToken)
	{
		var currentUser = currentUserService.GetCurrentUser();
		var user = context.Users
			.Include(u => u.Profile)
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
		if (request.ProfileImage != null)
		{
			var relativePath = await fileUploadService.UploadWebAsset(request.ProfileImage);

			user.Profile.ProfileImagePath = relativePath;

			await context.SaveChangesAsync(cancellationToken);
		}
	}
}