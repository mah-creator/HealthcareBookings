using HealthcareBookings.Domain.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HealthcareBookings.Application.Clinics.Queries;

public class GetClinicsQuery : IRequest<IQueryable<Clinic>>
{
	public string QueryParameter { get; set; }
	public string SortBy { get; set; }
	public string SortColumn { get; set; }
	[Required]
	public int PageSize { get; set; }
	[Required]
	public int Page { get; set; }
};
