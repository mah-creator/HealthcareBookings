using Microsoft.AspNetCore.Mvc;

namespace HealthcareBookings.API.Controllers.TermsAndConditions;

[Controller]
public class TermsConditionsController : ControllerBase
{
	private readonly IWebHostEnvironment _webHostEnvironment;

	public TermsConditionsController(IWebHostEnvironment webHostEnvironment)
	{
		_webHostEnvironment = webHostEnvironment;
	}

	[HttpGet]
	[Route("/terms-and-conditions")]
	public ContentResult GetTermsAndConditionsHtmlPage()
	{
		var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "terms-and-conditions.html");
		if (!System.IO.File.Exists(filePath))
		{
			return Content("Error: HTML file not found.", "text/plain");
		}

		var htmlContent = System.IO.File.ReadAllText(filePath);
		return Content(htmlContent, "text/html");
	}
}
