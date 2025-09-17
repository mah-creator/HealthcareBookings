using HealthcareBookings.Application.Doctors.Queries;
using HealthcareBookings.Domain.Entities;

namespace HealthcareBookings.Application.Doctors;

public static class Utils
{
	public static DoctorDto CreateDoctorDto(Doctor d, User patient)
	{
		return new DoctorDto
		{
			Id = d.DoctorUID,
			Name = d.Account?.Profile?.Name,
			IsFavorite = patient.PatientProperties?.FavoriteDoctors?.Find(fd => fd.DoctorID == d.DoctorUID) is not null,
			ClinicName = d.Clinic?.ClinicName,
			ClinicLocation = d.Clinic?.Location?.ToString(),
			Rating = d.Rating,
			Reviews = d.Appointments?.Count(a => a.Review != null)?? 0,
			Experience = d.ExperienceYears,
			Bio = d.Bio,
			PatientCount = d.Appointments?.DistinctBy(a => a.PatientID).Count() ?? 0
		};
	}
}
