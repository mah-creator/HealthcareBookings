using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.StaticFiles.Defaults;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBookings.Application.Clinics.Commands.Profile;

public class UpdateClinicProfileCommandHandler(
	IAppDbContext dbContext,
	CurrentUserEntityService currentUserEntityService) : IRequestHandler<UpdateClinicProfileCommand, ClinicProfileDto>
{
	public async Task<ClinicProfileDto> Handle(UpdateClinicProfileCommand request, CancellationToken cancellationToken)
	{
		var clinicAdmin = await currentUserEntityService.GetCurrentClinicAdmin();
		var clinic = clinicAdmin.ClinicAdminProperties?.Clinic;

		if (clinic == null)
		{
			throw new InvalidHttpActionException("clinic profile wasn't created");
		}

		clinic.ClinicName = request.Name ?? clinic.ClinicName;
		clinic.ClinicDescription = request.Description ?? clinic.ClinicDescription;
		clinic.Location = request.Location ?? clinic.Location;

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
