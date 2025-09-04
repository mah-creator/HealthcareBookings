using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Infrastructure.Persistence;
using HealthcareBookings.Infrastructure.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthcareBookings.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
	public static void AddInfrastucture(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<IAppDbContext, AppDbContext>(

		   options => options.UseSqlite(configuration.GetConnectionString("Sqlite"))
	   );

		services.AddScoped<IAppSeeder, AppSeeder>();
	}
}
