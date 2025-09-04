using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HealthcareBookings.Application.Doctors.Commands;

public class RegisterDoctorCommand : IRequest
{
	[EmailAddress]
	public string Email { get; set; }
	public string Password { get; set; }
	public string Category { get; set; }
}
