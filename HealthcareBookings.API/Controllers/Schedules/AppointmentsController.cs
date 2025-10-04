using HealthcareBookings.Application.Constants;
using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Doctors.Extensions;
using HealthcareBookings.Application.Extensions;
using HealthcareBookings.Application.Paging;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBookings.API.Controllers.Schedules;

[Controller]
[Route("/api/appointments")]
public class AppointmentsController(IAppDbContext dbContext, CurrentUserEntityService currentUserEntityService) : ControllerBase
{
	[HttpPost("{doctorId}")]
	[Authorize(Roles = UserRoles.Patient)]
	[ProducesResponseType(typeof(PatientAppointmentDto), 200)]
	[ProducesResponseType(typeof(string), 400)]
	public async Task<IActionResult> CreateAppointment(string doctorId, DateOnly date, TimeOnly time)
	{
		var success = createAppointmentAction(doctorId, date, time, out string? errorMessage, out PatientAppointmentDto? a);

		if (!success) throw new InvalidHttpActionException(errorMessage);

		return Ok(a);
	}

	[HttpPatch("reschedule/{appointmentId}")]
	[ProducesResponseType(typeof(PatientAppointmentDto), 200)]
	[ProducesResponseType(typeof(string), 400)]
	public async Task<IActionResult> RescheduleAppointment(string appointmentId, DateOnly date, TimeOnly time)
	{
		var transaction = dbContext.Database.BeginTransaction();
		transaction.CreateSavepoint("BeforeRescheduling");

		var appointment = dbContext.Appointments
			.Include(a => a.TimeSlot).Include(a => a.Doctor)
			.Where(a => a.AppointmentID == appointmentId)
			.FirstOrDefault();

		if (appointment == null || appointment.TimeSlot == null)
		{
			throw new InvalidHttpActionException($"Appointment in quesiton wasn't found");
		}
		if (!appointment.Status.Equals(AppointmentStatus.Upcoming, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidHttpActionException("Can't reschedule a completed/canceled appointment");
		}

		appointment.TimeSlot.IsFree = true;
		
		dbContext.Appointments.Remove(appointment);
		await dbContext.SaveChangesAsync();

		var success = createAppointmentAction(appointment.Doctor.DoctorUID, date, time, out string? errorMessage, out PatientAppointmentDto? a);

		if (!success)
		{
			transaction.RollbackToSavepoint("BeforeRescheduling");
			throw new InvalidHttpActionException(errorMessage);
		}
		return Ok(a);
	}

	[HttpGet("doctor")]
	[Authorize(Roles = UserRoles.Doctor)]
	public async Task<IActionResult> GetDoctorAppointments(string appointmentStatus, int page = 0, int pageSize = 0)
	{
		var doctor = await currentUserEntityService.GetCurrentDoctor();
		appointmentStatus = appointmentStatus?.ToLower();

		var appointments = dbContext.Appointments
			.Where(		a => a.Status.ToLower().Equals(appointmentStatus)
					&&	a.DoctorID == doctor.Id
					&&	a.TimeSlot.Schedule.Date == DateOnly.FromDateTime(DateTime.Today))
			.Include(a => a.Patient).ThenInclude(p => p.Account).ThenInclude(a => a.Profile)
			.Include(a => a.TimeSlot).ThenInclude(ts => ts.Schedule).AsEnumerable()
			.Where(a => !isOverdue(a))
			.Select(a => new AppointmentDto
			{
				AppointmentId = a.AppointmentID,
				Date = a.TimeSlot.Schedule.Date,
				Start = a.TimeSlot.StartTime,
				End = a.TimeSlot.EndTime,
				IsOverdue = isOverdue(a),
				PatientInfo = new PatientAppointmentInfo()
				{
					Name = a.Patient.Account.Profile.Name,
					Age = DateTime.Now.Year - a.Patient.Account.Profile.DOB.Year,
					Gender = a.Patient.Account.Profile.Gender
				}
			}).OrderBy(a => new DateTime(a.Date, a.Start)).AsQueryable();

		if (page != 0 && pageSize != 0)
			return Ok(PagedList<AppointmentDto>.CreatePagedList(appointments, page, pageSize));
		else
			return Ok(appointments?.ToList() ?? []);
	}

	[HttpGet("patient")]
	[Authorize(Roles = UserRoles.Patient)]
	public async Task<IActionResult> GetPatientAppointments(string appointmentStatus, int page = 0, int pageSize = 0)
	{
		var patient = await currentUserEntityService.GetCurrentPatient();
		var appointments = patient?.PatientProperties?.Appointments
			?.Where(a => 
				a.Status.Equals(appointmentStatus, StringComparison.OrdinalIgnoreCase))
			.AsEnumerable()
			.Where(a => !isOverdue(a))

			.Select(a => new PatientAppointmentDto
			{
				AppointmentId = a.AppointmentID,
				Date = a.TimeSlot.Schedule.Date,
				Start = a.TimeSlot.StartTime,
				End = a.TimeSlot.EndTime,
				ClinicName = a.Doctor.Clinic.ClinicName,
				ClinicLocation = a.Doctor.Clinic.Location.AddressText,
				DoctorName = a.Doctor.Account.Profile.Name,
				DoctorCategory = a.Doctor.Category.CategoryName,
				DoctorImage = ApiSettings.BaseUrl + a.Doctor.Account.Profile.ProfileImagePath,
				IsOverdue = isOverdue(a)
			}).OrderBy(a => new DateTime(a.Date, a.Start)).AsQueryable();

		if (page != 0 && pageSize != 0)
			return Ok(PagedList<PatientAppointmentDto>.CreatePagedList(appointments, page, pageSize));
		else
			return Ok(appointments?.ToList() ?? []);
	}

	[HttpDelete("cancel/{appointmentId}")]
	[ProducesResponseType(200)]
	[ProducesResponseType(typeof(string), 400)]
	public async Task<IActionResult> CancelAppointment(string appointmentId)
	{
		var appointment = dbContext.Appointments
			.Include(a => a.TimeSlot).Include(a => a.Doctor)
			.Where(a => a.AppointmentID == appointmentId)
			.FirstOrDefault();

		if (appointment == null || appointment.TimeSlot == null)
		{
			throw new InvalidHttpActionException("Appointment wasn't found");
		}
		if (!appointment.Status.Equals(AppointmentStatus.Upcoming, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidHttpActionException("Can't cancel a completed/cancled appointment");
		}

		appointment.TimeSlot.IsFree = true;
		appointment.Status = AppointmentStatus.Canceled;
		
		await dbContext.SaveChangesAsync();
		return Ok();
	}

	[HttpPost("complete/{appointmentId}")]
	public async Task<Results<Ok<AppointmentDto>, BadRequest<string>>> CompleteAppointment(string appointmentId)
	{
		var a = dbContext.Appointments
			.Include(a => a.Patient).ThenInclude(p => p.Account).ThenInclude(a => a.Profile)
			.Include(a => a.TimeSlot).ThenInclude(ts => ts.Schedule)
			.FirstOrDefault(a => a.AppointmentID == appointmentId);

		if (a == null) 
			throw new InvalidHttpActionException("Appointment wasn't found");

		if (!a.Status.Equals(AppointmentStatus.Upcoming, StringComparison.OrdinalIgnoreCase))
			throw new InvalidHttpActionException("Can't complete a cancled/completed appointmnet");

		if (!isOverdue(a))
			throw new InvalidHttpActionException("Appointmnet hasn't finished yet");

		a.Status = AppointmentStatus.Completed;
		await dbContext.SaveChangesAsync();

		return TypedResults.Ok(new AppointmentDto
		{
			AppointmentId = a.AppointmentID,
			Date = a.TimeSlot.Schedule.Date,
			Start = a.TimeSlot.StartTime,
			End = a.TimeSlot.EndTime,
			IsOverdue = isOverdue(a),
			PatientInfo = new PatientAppointmentInfo()
			{
				Name = a.Patient.Account.Profile.Name,
				Age = DateTime.Now.Year - a.Patient.Account.Profile.DOB.Year,
				Gender = a.Patient.Account.Profile.Gender
			}
		});
	}
	private  bool createAppointmentAction(string doctorId, DateOnly date, TimeOnly time,  out string? message, out PatientAppointmentDto? a)
	{
		message = null;
		a = null;

		var dateTime = new DateTime(date, time);

		if (dateTime < DateTime.Now - TimeSpan.FromHours(24))
		{
			
			message = "Appointments are only accepted 24 hours ahead";
			return false;
		}

		var schedule = dbContext.DoctorSchedules
			.Where(s => s.DoctorID == doctorId && s.Date == date)
			.Include(s => s.TimeSlots)
			.FirstOrDefault();

		var timeSlot = schedule?.TimeSlots.FirstOrDefault(s => s.StartTime == time && s.IsFree);

		if (timeSlot == null)
		{
			message = $"Doctor isn't available at {date}, {time}";
			return false;
		}

		var patient =  currentUserEntityService.GetCurrentPatient().Result;
		var patientAppointments = patient.PatientProperties.Appointments;
		if (patientAppointments.Where(a => a.TimeSlotID == timeSlot.SlotID && a.Status == AppointmentStatus.Upcoming && !isOverdue(a)).Any())
		{
			message = $"You already have an appointment at {schedule.Date}, {timeSlot.StartTime} - {timeSlot.EndTime}";
			return false;
		}
		if (patientAppointments.Where(a => a.DoctorID == doctorId).Any() && patientAppointments.Where(a => a.DoctorID == doctorId).Any(a => a.Status.Equals(AppointmentStatus.Upcoming) && !isOverdue(a)))
		{
			message = $"You already have an appointment with the doctor";
			return false;
		}


		var appointment = new Appointment
		{
			PatientID = patient.Id,
			DoctorID = doctorId,
			TimeSlotID = timeSlot.SlotID,
		};
		timeSlot.IsFree = false;
		patientAppointments.Add(appointment);
		dbContext.SaveChangesAsync();

		var doctor = dbContext.Doctors.IncludeAll().FirstOrDefault(d => d.DoctorUID == doctorId);

		a = new PatientAppointmentDto

		{
			AppointmentId = appointment.AppointmentID,
			Date = schedule.Date,
			Start = timeSlot.StartTime,
			End = timeSlot.EndTime,
			ClinicName = doctor.Clinic.ClinicName,
			ClinicLocation = doctor.Clinic.Location.AddressText,
			DoctorName = doctor.Account.Profile.Name,
			DoctorImage = doctor.Account.Profile.ProfileImagePath,
			DoctorCategory = doctor.Category.CategoryName
		};

		return true; 
	}

	private bool isNotPast(Appointment a)
	{
		var d = new DateTime(a.TimeSlot.Schedule.Date, a.TimeSlot.StartTime);
		return d > DateTime.Now.AddHours(24);
	}

	private static bool isOverdue(Appointment a)
	{
		var d = new DateTime(a.TimeSlot.Schedule.Date, a.TimeSlot.EndTime);
		return d <= DateTime.Now;
	}
}

internal struct PatientAppointmentDto
{
	public string AppointmentId { get; set; }
	public DateOnly Date { get; set; }
	public TimeOnly Start { get; set; }
	public TimeOnly End { get; set; }
	public string ClinicName { get; set; }
	public string ClinicLocation { get; set; }
	public string DoctorName { get; set; }
	public string DoctorCategory { get; set; }
	public string DoctorImage { get; set; }
	public bool IsOverdue { get; set; }
}

public struct AppointmentDto
{
	public string AppointmentId { get; set; }
	public DateOnly Date { get; set; }
	public TimeOnly Start { get; set; }
	public TimeOnly End { get; set; }
	public bool IsOverdue { get; set; }
	public PatientAppointmentInfo PatientInfo { get; set; }
}

public struct PatientAppointmentInfo
{
	public string Name { get; set; }
	public int Age { get; set; }
	public string Gender { get; set; }
}

