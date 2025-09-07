using HealthcareBookings.Application.Data;
using HealthcareBookings.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareBookings.API.Controllers.Miscellaneous;

[Controller]
[Route("/api/banners")]
public class BannersController(IAppDbContext dbContext) : ControllerBase
{
	[HttpGet]
	[ProducesResponseType(typeof(List<Banner>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public IActionResult GetBanners()
	{
		return dbContext.Banners.Any() ? 
			Ok(dbContext.Banners.ToList()) : 
			BadRequest();
	}
}
