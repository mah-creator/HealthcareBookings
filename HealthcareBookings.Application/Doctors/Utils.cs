using HealthcareBookings.Application.Constants;
using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Doctors.Queries;
using HealthcareBookings.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static HealthcareBookings.Application.Clinics.Utils;

namespace HealthcareBookings.Application.Doctors;

public static class Utils
{
	public static DoctorDto CreateDoctorDto(Doctor d, User patient, IAppDbContext dbContext)
	{
		var slot = nextAvailableSlot(d.DoctorUID, dbContext);
		return new DoctorDto
		{
			Id = d.DoctorUID,
			Name = d.Account?.Profile?.Name,
			Image = ApiSettings.BaseUrl + d.Account?.Profile?.ProfileImagePath,
			IsFavorite = patient.PatientProperties?.FavoriteDoctors?.Find(fd => fd.DoctorID == d.DoctorUID) is not null,
			Clinic = CreateClinicDto(d.Clinic, null, null, null),
			ClinicName = d.Clinic?.ClinicName,
			ClinicLocation = d.Clinic?.Location?.ToString(),
			Rating = d.Rating,
			Reviews = d.Appointments?.Count(a => a.Review != null) ?? 0,
			Experience = d.ExperienceYears,
			Bio = d.Bio,
			PatientCount = d.Appointments?.DistinctBy(a => a.PatientID).Count() ?? 0,
			ClosestAvailability = slot == null ?
				"Not available soon" :
				$"{GetDateLabel(slot.Value.Date)}, {slot?.StartTime} - {slot?.EndTime}"
		};
	}

	private static AvailableSlot? nextAvailableSlot(string docId, IAppDbContext dbContext)
	{
		const string sql = @"
		SELECT DoctorId, ScheduleID, StartTime, EndTime, Date
		FROM (
			SELECT 
				ds.DoctorId,
				ds.ScheduleID,
				ds.Date,
				dts.StartTime,
				dts.EndTime,
				ROW_NUMBER() OVER (PARTITION BY ds.DoctorId ORDER BY ds.Date, dts.StartTime) AS rn
			FROM DoctorTimeSlots dts
			JOIN DoctorSchedules ds ON ds.ScheduleID = dts.ScheduleID
			WHERE dts.IsFree = 1 AND ds.Date > DATE('now')  -- or GETDATE() for SQL Server
		) AS ranked
		WHERE rn = 1 and ranked.DoctorId = @docId
		ORDER BY DoctorId";

		var nextAvailableSlot = dbContext.DoctorTimeSlots.FromSqlRaw(sql, new SqliteParameter("@docId", docId))
			.Select(s => new AvailableSlot
			{
				ScheduleID = s.ScheduleID,
				DoctorID = s.Schedule.DoctorID,
				Date = s.Schedule.Date,
				StartTime = s.StartTime,
				EndTime = s.EndTime
			}).FirstOrDefault();

		return nextAvailableSlot;
	}
	private struct AvailableSlot
	{
		public string DoctorID;
		public string ScheduleID;
		public DateOnly Date;
		public TimeOnly StartTime;
		public TimeOnly EndTime;
	}

	public static string GetDateLabel(DateOnly date)
	{
		var today = DateOnly.FromDateTime(DateTime.Today);
		var tomorrow = today.AddDays(1);

		if (date == today)
			return "Today";
		if (date == tomorrow)
			return "Tomorrow";

		var dayOfWeek = date.DayOfWeek;
		var daysAhead = date.DayNumber - today.DayNumber;

		// If within next 5 days and before or on Friday
		if (daysAhead > 1 && daysAhead <= 5 && dayOfWeek <= DayOfWeek.Friday)
			return dayOfWeek.ToString(); // e.g. "Wednesday"

		// Otherwise return the date itself
		return date.ToString();
	}

}
