using HealthcareBookings.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareBookings.Application.Patients.Commands.Profile;

public class CreatePatientProfileCommand : IRequest
{
	public string Name { get; set; }
	public string Gender { get; set; }
	public DateOnly? DateOfBirth { get; set; }
	public IFormFile? ProfileImage { get; set; }
}