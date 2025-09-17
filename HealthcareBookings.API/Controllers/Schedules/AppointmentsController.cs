using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace HealthcareBookings.API.Controllers.Schedules;

[Controller]
[Route("/api/appointments")]
public class AppointmentsController(IAppDbContext dbContext, CurrentUserEntityService currentUserEntityService) : ControllerBase
{
	[HttpPost("{doctorId}")]
	[Authorize(Roles = UserRoles.Patient)]
	public async Task<IActionResult> CreateAppointment(string doctorId, DateOnly date, TimeOnly time)
	{
		var dateTime = new DateTime(date, time);
		if (dateTime < DateTime.Now - TimeSpan.FromHours(24))
		{
			return BadRequest("Appointments are only accepted before 24 hours");
		}

		var schedule = dbContext.DoctorSchedules
			.Where(s => s.DoctorID == doctorId && s.Date == date)
			.Include(s => s.TimeSlots.Where(ts => ts.StartTime == time))
			.FirstOrDefault();

		var timeSlot = schedule?.TimeSlots.FirstOrDefault();

		if (timeSlot == null)
		{
			return BadRequest("Appointments aren't available within the requested time");
		}
		if (!timeSlot.IsFree)
		{
			return BadRequest("Requested time slot is reserved");
		}

		var patient = await currentUserEntityService.GetCurrentPatient();
		var patientAppointments = patient.PatientProperties.Appointments;
		if (patientAppointments.Where(a => a.TimeSlotID == timeSlot.SlotID).Any())
		{
			return BadRequest("You already have an appointment at this time");
		}
		if (patientAppointments.Where(a => a.DoctorID == doctorId).Any() && patientAppointments.Where(a => a.DoctorID == doctorId).All(a => a.Status.Equals(AppointmentStatus.Upcoming)))
		{
			return BadRequest("You have an appointment with this doctor");
		}

		patientAppointments.Add(new()
		{
			PatientID = patient.Id,
			DoctorID = doctorId,
			TimeSlotID = timeSlot.SlotID,
		});
		timeSlot.IsFree = false;
		await dbContext.SaveChangesAsync();

		var newAppointment = patientAppointments.Last();

		return Ok(new PatientAppointmentDto
		{
			AppointmentId = newAppointment.AppointmentID,
			Date = newAppointment.TimeSlot.Schedule.Date,
			Start = newAppointment.TimeSlot.StartTime,
			End = newAppointment.TimeSlot.EndTime,
			ClinicName = newAppointment.Doctor.Clinic.ClinicName,
			DoctorName = newAppointment.Doctor.Account.Profile.Name,
			DoctorCategory = newAppointment.Doctor.Category.CategoryName
		});
	}

	[HttpPatch("reschedule/{appointmentId}")]
	public async Task<IActionResult> RescheduleAppointment(string appointmentId, DateOnly date, TimeOnly time)
	{
		var appointment = dbContext.Appointments
			.Include(a => a.TimeSlot).Include(a => a.Doctor)
			.Where(a => a.AppointmentID == appointmentId)
			.FirstOrDefault();

		if (appointment == null || appointment.TimeSlot == null)
		{
			return BadRequest("Appointment wasn't found");
		}
		if (!appointment.Status.Equals(AppointmentStatus.Upcoming, StringComparison.OrdinalIgnoreCase))
		{
			return BadRequest("Can't reschedule a completed/canceled appointment");
		}

		appointment.TimeSlot.IsFree = true;
		
		dbContext.Appointments.Remove(appointment);
		await dbContext.SaveChangesAsync();

		return await CreateAppointment(appointment.Doctor.DoctorUID, date, time);
	}

	[HttpGet("doctor")]
	[Authorize(Roles = UserRoles.Doctor)]
	public async Task<IActionResult> GetDoctorAppointments(string appointmentStatus)
	{
		var doctor = await currentUserEntityService.GetCurrentDoctor();

		var appointments = dbContext.Appointments
			.Where(		a => a.Status.ToLower().Equals(appointmentStatus.ToLower())
					&&	a.DoctorID == doctor.Id)
			.Include(a => a.Patient).ThenInclude(p => p.Account).ThenInclude(a => a.Profile)
			.Include(a => a.TimeSlot).ThenInclude(ts => ts.Schedule)
			.Select(a => new AppointmentDto
			{
				AppointmentId = a.AppointmentID,
				Date = a.TimeSlot.Schedule.Date,
				Start = a.TimeSlot.StartTime,
				End = a.TimeSlot.EndTime,
				PatientInfo = new PatientAppointmentInfo()
				{
					Name = a.Patient.Account.Profile.Name,
					Age = DateTime.Now.Year - a.Patient.Account.Profile.DOB.Year,
					Gender = a.Patient.Account.Profile.Gender
				}
			}).ToList();

		return Ok(appointments);
	}

	[HttpGet("patient")]
	[Authorize(Roles = UserRoles.Patient)]
	public async Task<IActionResult> GetPatientAppointments(string appointmentStatus)
	{
		var patient = await currentUserEntityService.GetCurrentPatient();
		var appointments = patient.PatientProperties.Appointments
			.Where(a => a.Status.Equals(appointmentStatus, StringComparison.OrdinalIgnoreCase))

			.Select(a => new PatientAppointmentDto
			{
				AppointmentId = a.AppointmentID,
				Date = a.TimeSlot.Schedule.Date,
				Start = a.TimeSlot.StartTime,
				End = a.TimeSlot.EndTime,
				ClinicName = a.Doctor.Clinic.ClinicName,
				DoctorName = a.Doctor.Account.Profile.Name,
				DoctorCategory = a.Doctor.Category.CategoryName
			}).ToList();

		return Ok(appointments);
	}

	[HttpDelete("cancel/{appointmentId}")]
	public async Task<IActionResult> CancelAppointment(string appointmentId)
	{
		var appointment = dbContext.Appointments
			.Include(a => a.TimeSlot).Include(a => a.Doctor)
			.Where(a => a.AppointmentID == appointmentId)
			.FirstOrDefault();

		if (appointment == null || appointment.TimeSlot == null)
		{
			return BadRequest("Appointment wasn't found");
		}
		if (!appointment.Status.Equals(AppointmentStatus.Upcoming, StringComparison.OrdinalIgnoreCase))
		{
			return BadRequest("Can't cancel a completed/cancled appointment");
		}

		appointment.TimeSlot.IsFree = true;

		dbContext.Appointments.Remove(appointment);
		await dbContext.SaveChangesAsync();

		return Ok();
	}
}

internal struct PatientAppointmentDto
{
	public string AppointmentId { get; set; }
	public DateOnly Date { get; set; }
	public TimeOnly Start { get; set; }
	public TimeOnly End { get; set; }
	public string ClinicName { get; set; }
	public string DoctorName { get; set; }
	public string DoctorCategory { get; set; }
}

internal struct AppointmentDto
{
	public string AppointmentId { get; set; }
	public DateOnly Date { get; set; }
	public TimeOnly Start { get; set; }
	public TimeOnly End { get; set; }
	public PatientAppointmentInfo PatientInfo { get; set; }
}

internal struct PatientAppointmentInfo
{
	public string Name { get; set; }
	public int Age { get; set; }
	public string Gender { get; set; }
}
