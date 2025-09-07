using HealthcareBookings.Application.Data;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareBookings.API.Controllers.Miscellaneous;

[Controller]
[Route("api/categories")]
public class CategoriesController(IAppDbContext dbContext) : ControllerBase
{
	[HttpGet]
	[ProducesResponseType(typeof(List<CategorieDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public IActionResult GetCategories()
	{
		return dbContext.DoctorCategories.Any() ?

			Ok(dbContext.DoctorCategories.Select(c => new CategorieDto
			{
				Id = c.CategoryID,
				Name = c.CategoryName,
				LogoPath = c.CategoryLogoPath!
			}))
			: BadRequest();
	}
}

internal struct CategorieDto
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string LogoPath { get; set; }
}