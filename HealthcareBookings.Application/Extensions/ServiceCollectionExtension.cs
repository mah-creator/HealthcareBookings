using HealthcareBookings.Application.Doctor.Commands;
using HealthcareBookings.Application.Middleware;
using HealthcareBookings.Application.Patient.Commands;
using HealthcareBookings.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace HealthcareBookings.Application.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
	   var registerPatientCommandHandlerAssembly = typeof(RegisterPatientCommandHandler).Assembly;
	   var registerDoctorCommandHandlerAssembly = typeof(RegisterDoctorCommandHandler).Assembly;

		services.AddMediatR(cfg =>
	   {
		  cfg.RegisterServicesFromAssemblies(
			 [
				registerPatientCommandHandlerAssembly,
				registerDoctorCommandHandlerAssembly
			 ]
		  );
	   });

		services.AddScoped<ExceptionHandlingMiddleawre>();
		services.AddScoped<UserRegistrationService>();
    }
}
