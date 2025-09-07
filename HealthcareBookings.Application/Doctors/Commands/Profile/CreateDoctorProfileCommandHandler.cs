using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Application.Data;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Exceptions;
using HealthcareBookings.Application.StaticFiles.Uploads;
using HealthcareBookings.Application.StaticFiles.Defaults;

namespace HealthcareBookings.Application.Doctors.Commands.Profile;

public class CreateDoctorProfileCommandHandler(
	CurrentUserEntityService currentUserEntityService,
	FileUploadService fileUploadService,
	DefaultProfileImageService defaultProfileImageService,
	IAppDbContext dbContext) : IRequestHandler<CreateDoctorProfileCommand>
{
	public async Task Handle(CreateDoctorProfileCommand request, CancellationToken cancellationToken)
	{
		var doctor = await currentUserEntityService.GetCurrentDoctor();

		if (doctor.Profile is not null)
		{
			throw new InvalidHttpActionException($"Doctor already has a profile");
		}
		if (doctor.DoctorProperties is null)
		{
			throw new InvalidHttpActionException("Doctor wasn't registered successfully");
		}

		string relativePath;
		if (request.ProfileImage is null)
		{
			relativePath = defaultProfileImageService.GetDefaultProfileImage(UserRoles.Doctor);
		}
		else
		{
			relativePath = await fileUploadService.UploadWebAsset(request.ProfileImage);
		}

		doctor.Profile = new ProfileInformation
		{
			UserID = doctor.Id,
			DOB = request.DateOfBirth,
			Gender = request.Gender,
			Name = request.Name,
			ProfileImagePath = relativePath
		};


		doctor.DoctorProperties.ExperienceYears = request.ExperienceYears;
		doctor.DoctorProperties.Bio = request.Bio;

		await dbContext.SaveChangesAsync(cancellationToken);
	}
}
