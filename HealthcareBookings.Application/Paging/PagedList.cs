using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace HealthcareBookings.Application.Paging;

public class PagedList<T>
{
	public List<T> Items { get; set; }
	public int Page {  get; set; }
	public int PageSize { get; set; }
	public int TotalCount { get; set; }
	public bool HasNext => Page * PageSize < TotalCount;
	public bool HasPrevious => Page > 1;

	public static PagedList<T> CreatePagedList(IQueryable<T>? query , int page = 1, int pageSize = 20)
	{
		var items = query?
			.Skip((page - 1) * pageSize)
			.Take(pageSize);

		return new()
		{
			Items = items?.ToList() ?? [],
			Page = page,
			PageSize = pageSize,
			TotalCount = query?.Count() ?? 0
		};
	}
}
