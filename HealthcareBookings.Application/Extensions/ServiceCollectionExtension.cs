using HealthcareBookings.Application.Doctors.Commands;
using HealthcareBookings.Application.Middleware;
using HealthcareBookings.Application.Patients.Commands;
using HealthcareBookings.Application.Patients.Commands.Profile;
using HealthcareBookings.Application.StaticFiles.Defaults;
using HealthcareBookings.Application.StaticFiles.Uploads;
using HealthcareBookings.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace HealthcareBookings.Application.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
	   var registerDoctorCommandHandlerAssembly = typeof(RegisterDoctorCommandHandler).Assembly;
	   var createPatientProfileCommandHandlerAssembly = typeof(CreatePatientProfileCommandHandler).Assembly;

		services.AddMediatR(cfg =>
	   {
		  cfg.RegisterServicesFromAssemblies(
			 [
				registerDoctorCommandHandlerAssembly,
				createPatientProfileCommandHandlerAssembly
			 ]
		  );
	   });

		services.AddScoped<ExceptionHandlingMiddleawre>();
		services.AddScoped<UserRegistrationService>();
		services.AddScoped<CurrentUserService>();
		services.AddScoped<CurrentUserEntityService>();
		services.AddScoped<FileUploadService>();
		services.AddScoped<DefaultProfileImageService>();
    }
}
