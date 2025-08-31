using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using MediatR;

namespace HealthcareBookings.Application.Doctor.Commands;

public class RegisterDoctorCommandHandler(UserRegistrationService userRegistrationService) : IRequestHandler<RegisterDoctorCommand>
{
	public async Task Handle(RegisterDoctorCommand request, CancellationToken cancellationToken)
	{
		await userRegistrationService.RegisterUser(
			email: request.Email,
			password: request.Password,
			role: UserRoles.Doctor,
			cancellationToken);
	}
}
