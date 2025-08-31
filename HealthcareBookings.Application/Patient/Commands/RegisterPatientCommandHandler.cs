using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace HealthcareBookings.Application.Patient.Commands;

public class RegisterPatientCommandHandler(UserRegistrationService userRegistrationService) : IRequestHandler<RegisterPatientCommand>
{
	public async Task Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
	{
		await userRegistrationService.RegisterUser(
			email: request.Email,
			password: request.Password,
			role: UserRoles.Patient,
			cancellationToken);
	}
}
