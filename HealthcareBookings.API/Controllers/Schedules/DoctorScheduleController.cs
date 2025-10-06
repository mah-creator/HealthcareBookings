using FluentValidation;
using FluentValidation.Validators;
using HealthcareBookings.Application.Data;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace HealthcareBookings.API.Controllers.Schedules;

[Controller]
[Route("/api/schedules")]
public class DoctorScheduleController(IAppDbContext dbContext) : ControllerBase
{
	[HttpGet("{doctorId}")]
	[ProducesResponseType(typeof(List<TimeSlotDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(string), 400)]
	[Authorize(Roles = $"{UserRoles.ClinicAdmin},{UserRoles.Patient}")]
	public async Task<IActionResult> GetSchedule(string doctorId, [Required] DateOnly date)
	{
		var doctor = dbContext.Doctors
			.Include(d => d.Schedules)
			.ThenInclude(s => s.TimeSlots)
			.Where(d => d.DoctorUID == doctorId)
			.FirstOrDefault();

		if (doctor == null)
		{
			throw new InvalidHttpActionException($"Doctor with id {doctorId} wasn't found");
		}

		var schedule = doctor.Schedules
			.Where(s => s.Date == date)
			.FirstOrDefault();

		return Ok(schedule?.TimeSlots?.Select(ts => new TimeSlotDto
		{
			Id = ts.SlotID,
			StartTime = ts.StartTime,
			EndTime = ts.EndTime,
			IsFree = ts.IsFree
		}) ?? []);
	}

	[HttpPost("{doctorId}")]
	[ProducesResponseType(typeof(List<TimeSlotDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
	[Authorize(Roles = UserRoles.ClinicAdmin)]
	public async Task<IActionResult> CreateTimeSlot(
		string doctorId,
		[Required] DateOnly date,
		[Required] TimeOnly start,
		[Required] TimeOnly end)
	{
		if (start >= end) throw new InvalidHttpActionException($"Invalid time interval");
		if (start.AddMinutes(10) > end) throw new InvalidHttpActionException($"Invalid time interval {start} - {end}: appointments are at least 10 minutes");

		var doctor = dbContext.Doctors
			.Include(d => d.Schedules)
			.ThenInclude(s => s.TimeSlots)
			.Where(d => d.DoctorUID == doctorId)
			.First();

		if (doctor == null)
		{
			throw new InvalidHttpActionException($"Doctor with id '{doctorId}' wasn't found");
		}

		var schedule = doctor.Schedules.Where(s => s.Date == date)?.FirstOrDefault();

		if (schedule == null)
		{
			schedule = new Schedule
			{
				Date = date,
				DoctorID = doctor.DoctorUID,
				TimeSlots = []
			};
			doctor.Schedules.Add(schedule);
		}

		var timeSlot = schedule.TimeSlots.Where(s => timeWithin(s.StartTime, s.EndTime, start) || timeWithin(s.StartTime, s.EndTime, end) || (s.StartTime==start && s.EndTime==end))?.FirstOrDefault();

		if (timeSlot != null)
			throw new InvalidHttpActionException($"Doctor already has a schedule entry at {date}, {timeSlot.StartTime} - {timeSlot.EndTime}");

		schedule.TimeSlots.Add(new TimeSlot()
		{
			ScheduleID = schedule.ScheduleID,
			StartTime = start,
			EndTime = end
		});

		await dbContext.SaveChangesAsync();

		return Ok(schedule?.TimeSlots.Select(ts => new TimeSlotDto
		{
			Id = ts.SlotID,
			StartTime = ts.StartTime,
			EndTime = ts.EndTime,
			IsFree = ts.IsFree
		}));
	}

	[HttpDelete("{slotId}")]
	[Authorize(Roles = UserRoles.ClinicAdmin)]
	public async Task<Results<Ok, BadRequest<string>>> DeleteTimeSlot(string slotId)
	{
		var slot = dbContext.DoctorTimeSlots.Find(slotId);

		if (slot == null) throw new InvalidHttpActionException("Timeslot wasn't found");
		if (!slot.IsFree) throw new InvalidHttpActionException($"Timeslot isn't marked free");

		dbContext.DoctorTimeSlots.Remove(slot);
		await dbContext.SaveChangesAsync();

		return TypedResults.Ok();
	}


	private bool timeWithin(TimeOnly start, TimeOnly end, TimeOnly time) => time < end && time > start;
}

internal struct TimeSlotDto
{
	public string Id { get; set; }
	public TimeOnly StartTime { get; set; }
	public TimeOnly EndTime { get; set; }
	public bool IsFree { get; set; }

}

