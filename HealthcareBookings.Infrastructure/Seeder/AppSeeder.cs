using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace HealthcareBookings.Infrastructure.Seeder;

public class AppSeeder(AppDbContext dbContext, IPasswordHasher<User> passwordHasher) : IAppSeeder
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
			if (!dbContext.Banners.Any())
			{
				var banners = GetBanners();
				dbContext.Banners.AddRange(banners);
				await dbContext.SaveChangesAsync();
			}
		}
	}

	private IEnumerable<Banner> GetBanners()
	{
		List<Banner> banners =
		[
			new()
			{
				ImagePath = "/images/defaults/banner1.png"
			},
			new() 
			{
				ImagePath = "/images/defaults/banner2.png"

			}
		];
		return banners;
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

	// Generate two clinic admins with navigation properties
	private List<User> GenerateTwoClinicAdmins()
	{
		var admin1 = new User
		{
			Id = Guid.NewGuid().ToString(),
			UserName = "clinicadmin1",
			NormalizedUserName = "CLINICADMIN1",
			Email = "clinicadmin1@healthcare.com",
			NormalizedEmail = "CLINICADMIN1@HEALTHCARE.COM",
			EmailConfirmed = true,
			PhoneNumber = "111222333",
			PhoneNumberConfirmed = true,
			SecurityStamp = Guid.NewGuid().ToString("D"),
			ClinicAdminProperties = new ClinicAdmin
			{
				ClinicAdminUID = "admin1-id",
				Clinic = new Clinic
				{
					ClinicID = Guid.NewGuid().ToString(),
					ClinicName = "Sunrise Health Clinic",
					ClinicDescription = "General healthcare and wellness services",
					ImagePath = "images/clinic1.png",
					Location = new Location
					{
						AddressText = "123 Main St",
						City = "MetroCity",
						PostalCode = "10001",
						StreetName = "Main St",
						StreetNumber = 123,
						Latitude = 40.7128F,
						Longitude = -74.0060F
					}
				}
			}
		};
		admin1.PasswordHash = passwordHasher.HashPassword(admin1, "123aA!");

		var admin2 = new User
		{
			Id = Guid.NewGuid().ToString(),
			UserName = "clinicadmin2",
			NormalizedUserName = "CLINICADMIN2",
			Email = "clinicadmin2@healthcare.com",
			NormalizedEmail = "CLINICADMIN2@HEALTHCARE.COM",
			EmailConfirmed = true,
			PhoneNumber = "444555666",
			PhoneNumberConfirmed = true,
			SecurityStamp = Guid.NewGuid().ToString("D"),
			ClinicAdminProperties = new ClinicAdmin
			{
				ClinicAdminUID = "admin2-id",
				Clinic = new Clinic
				{
					ClinicID = Guid.NewGuid().ToString(),
					ClinicName = "Evergreen Family Clinic",
					ClinicDescription = "Family medicine and pediatric care",
					ImagePath = "images/clinic2.png",
					Location = new Location
					{
						AddressText = "456 Oak Ave",
						City = "GreenTown",
						PostalCode = "20002",
						StreetName = "Oak Ave",
						StreetNumber = 456,
						Latitude = 34.0522F,
						Longitude = -118.2437F
					}
				}
			}
		};
		admin2.PasswordHash = passwordHasher.HashPassword(admin2, "123aA!");

		return new List<User> { admin1, admin2 };
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