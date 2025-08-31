using MediatR;

namespace HealthcareBookings.Application.Doctor.Commands;

public class RegisterDoctorCommand : IRequest
{
	public string Email { get; set; }
	public string Password { get; set; }
}
