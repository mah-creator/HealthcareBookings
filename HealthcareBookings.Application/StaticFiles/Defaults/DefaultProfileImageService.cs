using HealthcareBookings.Domain.Constants;

namespace HealthcareBookings.Application.StaticFiles.Defaults;

public class DefaultProfileImageService
{
	public string GetDefaultProfileImage(string userRole)
	{
		string defaultsFolder = "/images/defaults";
		return userRole switch
		{
			UserRoles.Patient => $"{defaultsFolder}/{DefaultProfileImages.PatientProfile}",
			UserRoles.Doctor => $"{defaultsFolder}/{DefaultProfileImages.DoctorProfile}",
			UserRoles.ClinicAdmin => $"{defaultsFolder}/{DefaultProfileImages.ClinicProfile}",
			_ => ""
		};
	}
}
