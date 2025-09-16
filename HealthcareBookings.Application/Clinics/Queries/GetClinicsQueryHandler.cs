using HealthcareBookings.Application.Data;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using MediatR;
using System.Linq.Expressions;

namespace HealthcareBookings.Application.Clinics.Queries;

public class GetClinicsQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetClinicsQuery, IQueryable<Clinic>>
{
	public async Task<IQueryable<Clinic>> Handle(GetClinicsQuery request, CancellationToken cancellationToken)
	{
		var clinicsQuery = dbContext.Clinics.AsQueryable();

		if (!string.IsNullOrEmpty(request.QueryParameter))
		{
			clinicsQuery = clinicsQuery
				.Where(c => c.ClinicName.ToLower()
				.Contains(request.QueryParameter.ToLower()));
		}

		Expression<Func<Clinic, object>> sortColumnSelection = request.SortColumn switch
		{
			"name" => c => c.ClinicName,
			_ => c => c.ClinicID
		};

		if (request.SortOrder == SortOrder.Desc)
			clinicsQuery = clinicsQuery.OrderByDescending(sortColumnSelection);
		else
			clinicsQuery = clinicsQuery.OrderBy(sortColumnSelection);

		return clinicsQuery;
	}
}
