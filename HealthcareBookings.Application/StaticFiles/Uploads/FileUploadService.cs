using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace HealthcareBookings.Application.StaticFiles.Uploads;

public class FileUploadService(IWebHostEnvironment environment)
{
	public async Task<string> UploadWebAsset(IFormFile staticFile)
	{
		var relativePath =
				$"images/uploads/{Guid.NewGuid().ToString()}_{staticFile.FileName}";

		var absolutePath = Path.Combine(
			environment.WebRootPath,
			relativePath);

		var streamWritter = new FileStream(absolutePath, FileMode.OpenOrCreate);
		await staticFile.CopyToAsync(streamWritter);

		return relativePath;
	}
}
