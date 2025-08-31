using MediatR;

namespace HealthcareBookings.Application.Patient.Commands;

public class RegisterPatientCommand : IRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
