using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HealthcareBookings.Application.Doctors.Commands.Profile;

public class CreateDoctorProfileCommand : IRequest
{
	public string Name { get; set; }
	public string Gender { get; set; }
	public DateOnly DateOfBirth { get; set; }
	public string? Bio {  get; set; }
	public int ExperienceYears { get; set; }
	public IFormFile? ProfileImage { get; set; }
}