using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace HealthcareBookings.Infrastructure.Seeder;

public class AppSeeder(AppDbContext dbContext) : IAppSeeder
{
	public async Task Seed()
	{
		if (await dbContext.Database.CanConnectAsync())
		{
			if (!dbContext.Roles.Any())
			{
				var roles = GetRoles();
				dbContext.Roles.AddRange(roles);
				await dbContext.SaveChangesAsync();
			}
			if (!dbContext.DoctorCategories.Any())
  			{
				var doctorCategories = GetDoctorCategories();
				dbContext.DoctorCategories.AddRange(doctorCategories);
				await dbContext.SaveChangesAsync();
			}
		}
	}

	private IEnumerable<DoctorCategory> GetDoctorCategories()
	{
		List<DoctorCategory> doctorCategories =
		[
			new()
			{
				CategoryName = "Dentistry",
				NormalizedName = "DENTISTRY",
				CategoryLogoPath = "/images/defaults/dentistry_category.png"
			},
			new()
			{
				CategoryName = "Cardiology",
				NormalizedName = "CARDIOLOGY",
				CategoryLogoPath = "/images/defaults/cardiology_category.png"
			},
			new() 
			{
				CategoryName = "Pulmonology",
				NormalizedName = "PULMONOLOGY",
				CategoryLogoPath = "/images/defaults/pulmonology_category.png"
			},
			new()
			{
				CategoryName = "General",
				NormalizedName = "GENERAL",
				CategoryLogoPath = "/images/defaults/general_category.png"
			},
			new()
			{
				CategoryName = "Neurology",
				NormalizedName = "NEUROLOGY",
				CategoryLogoPath = "/images/defaults/neurology_category.png"
			},
			new() 
			{
				CategoryName = "Gastroenterology",
				NormalizedName = "GASTROENTEROLOGY",
				CategoryLogoPath = "/images/defaults/gastroenterology_category.png"
			}
		];

		return doctorCategories;
	}

	private IEnumerable<IdentityRole> GetRoles()
	{
		/* NOTE. For each role, its normalized name is explicitly defined
		 * as it shouldn't be null in the DB because the RoleManager
		 * looks up roles against their normalized names 
		 */

		List<IdentityRole> roles =
		[
		   new (UserRoles.Patient)
		  {
			 Name = UserRoles.Patient,
			 NormalizedName = UserRoles.Patient.ToUpper()
		  },
		  new ()
		  {
			 Name = UserRoles.Admin,
			 NormalizedName = UserRoles.Admin.ToUpper()
		  },
		  new ()
		  {
			 Name = UserRoles.ClinicAdmin,
			 NormalizedName = UserRoles.ClinicAdmin.ToUpper()
		  },
		  new ()
		  {
			 Name = UserRoles.Doctor,
			 NormalizedName = UserRoles.Doctor.ToUpper()
		  }
		];

		return roles;
	}
}