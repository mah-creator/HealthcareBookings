using HealthcareBookings.Domain.Entities;
using MediatR;

namespace HealthcareBookings.Application.Clinics.Commands.Profile;

public class CreateClinicProfileCommand : IRequest
{
	public string Name { get; set; }
	public string Description { get; set; }
	public Location Location { get; set; }
}