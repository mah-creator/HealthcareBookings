using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Doctors.Extensions;
using HealthcareBookings.Application.Paging;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using MediatR;
using System.Linq.Expressions;
using static HealthcareBookings.Application.Doctors.Utils;

namespace HealthcareBookings.Application.Doctors.Queries;

public class GetDoctorsQueryHandler(
	IAppDbContext dbContext,
	CurrentUserEntityService currentUserEntityService) : IRequestHandler<GetDoctorsQuery, IQueryable<Doctor>>
{
	private User patient = currentUserEntityService.GetCurrentPatient().Result;

	public async Task<IQueryable<Doctor>> Handle(GetDoctorsQuery request, CancellationToken cancellationToken)
	{

		var doctorssQuery = dbContext.Doctors.IncludeAll().AsQueryable();

		Expression<Func<Doctor, object>> sortColumntSelector = request.SortColumn switch
		{
			"name" => d => d.Account.Profile.Name,
			"rating" => d => d.Appointments.Sum(a => a.Review.Rating) / d.Appointments.Count(a => a.Review != null),
			"reviews" => d => d.Appointments.Count(a => a.Review != null),
			_ => d => d.DoctorUID
		};

		if (!string.IsNullOrEmpty(request.QueryParameter))
		{
			doctorssQuery = doctorssQuery
				.Where(d => 
					d.Account.Profile.Name.ToLower()
					.Contains(request.QueryParameter!.ToLower()));
		}

		if (request.SortOrder == SortOrder.Desc)
			doctorssQuery = doctorssQuery.OrderByDescending(sortColumntSelector);
		else
			doctorssQuery = doctorssQuery.OrderBy(sortColumntSelector);

		return doctorssQuery;
	}
}
