using HealthcareBookings.Application.Middleware;
using HealthcareBookings.Application.Patient.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace HealthcareBookings.Application.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
	   var registerPatientCommandHandlerAssembly = typeof(RegisterPatientCommandHandler).Assembly;

	   services.AddMediatR(cfg =>
	   {
		  cfg.RegisterServicesFromAssemblies(
			 [
				registerPatientCommandHandlerAssembly
			 ]
		  );
	   });

		services.AddScoped<ExceptionHandlingMiddleawre>();
    }
}
