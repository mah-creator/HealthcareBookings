using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.StaticFiles.Defaults;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Domain.Exceptions;
using MediatR;

namespace HealthcareBookings.Application.Clinics.Commands.Profile;

public class CreateClinicProfileCommandHandler(
	IAppDbContext dbContext,
	DefaultProfileImageService defaultProfileImageService,
	CurrentUserEntityService currentUserEntityService) : IRequestHandler<CreateClinicProfileCommand, ClinicProfileDto>
{
	public async Task<ClinicProfileDto> Handle(CreateClinicProfileCommand request, CancellationToken cancellationToken)
	{
		var clinicAdmin = await currentUserEntityService.GetCurrentClinicAdmin();

		if (clinicAdmin.ClinicAdminProperties?.Clinic != null)
		{
			throw new InvalidHttpActionException("clinic profile was already created");
		}

		var clinic = new Clinic
		{
			ClinicName = request.Name,
			ClinicDescription = request.Description,
			ImagePath = defaultProfileImageService.GetDefaultProfileImage(UserRoles.ClinicAdmin),
			Location = request.Location
		};

		dbContext.Clinics.Add(clinic);

		clinicAdmin.ClinicAdminProperties = new ClinicAdmin
		{
			ClinicAdminUID = clinicAdmin.Id,
			ClinicID = clinic.ClinicID
		};


		await dbContext.SaveChangesAsync();

		return new()
		{
			Name = clinic.ClinicName,
			Description = clinic.ClinicDescription!,
			ProfileImagePath = clinic.ImagePath,
			Location = clinic.Location,
		};
	}
}
