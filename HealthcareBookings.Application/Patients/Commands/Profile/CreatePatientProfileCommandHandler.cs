using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Application.Data;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Exceptions;
using HealthcareBookings.Application.StaticFiles.Uploads;
using HealthcareBookings.Application.StaticFiles.Defaults;

namespace HealthcareBookings.Application.Patients.Commands.Profile;

public class CreatePatientProfileCommandHandler(
	CurrentUserService currentUserService,
	DefaultProfileImageService defaultProfileImageService,
	FileUploadService fileUploadService,
	IAppDbContext context) : IRequestHandler<CreatePatientProfileCommand>
{
	public async Task Handle(CreatePatientProfileCommand request, CancellationToken cancellationToken)
	{
		var currentUser = currentUserService.GetCurrentUser();

		var user = context.Users.Include(u => u.Profile)
			.Where(u => u.Id == currentUser.Id)
			.ToList()
			.First();

		if (user.Profile is not null)
		{
			throw new InvalidHttpActionException($"User already has a profile");
		}

		string relativePath;
		if (request.ProfileImage is null)
		{
			relativePath = defaultProfileImageService.GetDefaultProfileImage(UserRoles.Patient);
		}
		else
		{
			relativePath = await fileUploadService.UploadWebAsset(request.ProfileImage);
		}

		user.Profile = new ProfileInformation
		{
			DOB = request.DateOfBirth,
			Gender = request.Gender,
			Name = request.Name,
			ProfileImagePath = relativePath
		};

		await context.SaveChangesAsync(cancellationToken);
	}
}
