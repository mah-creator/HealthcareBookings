using HealthcareBookings.Application.Clinics;
using HealthcareBookings.Application.Paging;
using HealthcareBookings.Domain.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HealthcareBookings.Application.Doctors.Queries;

public class GetDoctorsQuery : IRequest<IQueryable<Doctor>>
{
	public string? QueryParameter { get; set; }
	public string? SortColumn { get; set; }
	public string? SortOrder { get; set; }
	[Required]
	public int PageSize { get; set; }
	[Required]
	public int Page { get; set; }
}

public struct DoctorDto
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string Image { get; set; }
	public bool IsFavorite { get; set; }
	public ClinicDto Clinic { get; set; }
	public string ClinicName { get; set; }
	public string ClinicLocation { get; set; }
	public float Rating { get; set; }
	public int Reviews { get; set; }
	public int Experience { get; set; }
	public int PatientCount { get; set; }
	public string Bio { get; set; }
}
