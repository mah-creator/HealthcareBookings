using FluentValidation;
using FluentValidation.Validators;
using HealthcareBookings.Application.Data;
using HealthcareBookings.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HealthcareBookings.API.Controllers.Schedules;

[Controller]
[Route("/api/schedules")]
public class DoctorScheduleController(IAppDbContext dbContext) : ControllerBase
{
	[HttpGet("{doctorId}")]
	[ProducesResponseType(typeof(List<TimeSlotDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetSchedule(string doctorId, [Required] DateOnly date)
	{
		var doctor = dbContext.Doctors
			.Include(d => d.Schedules)
			.ThenInclude(s => s.TimeSlots)
			.Where(d => d.DoctorUID == doctorId)
			.FirstOrDefault();

		if (doctor == null)
		{
			return NotFound("doctor doesn't exist");
		}

		var schedule = doctor.Schedules
			.Where(s => s.Date == date)
			.FirstOrDefault();

		return Ok(schedule?.TimeSlots.Select(ts => new TimeSlotDto
		{
			Id = ts.SlotID,
			StartTime = ts.StartTime,
			EndTime = ts.EndTime,
			IsFree = ts.IsFree
		}));
	}

	[HttpPost("{doctorId}")]
	[ProducesResponseType(typeof(List<TimeSlotDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> CreateTimeSlot(
		string doctorId,
		[Required] DateOnly date,
		[Required] TimeOnly start,
		[Required] TimeOnly end)
	{
		// TODO: validate start < end

		var doctor = dbContext.Doctors
			.Include(d => d.Schedules)
			.ThenInclude(s => s.TimeSlots)
			.Where(d => d.DoctorUID == doctorId)
			.First();

		if (doctor == null)
		{
			return BadRequest("doctor doesn't exist");
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

		var timeSlot = schedule.TimeSlots.Where(s => s.StartTime == start || s.EndTime == end)?.FirstOrDefault();

		if (timeSlot != null)
			return BadRequest("An entry with the same start/end date exists");

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
}

internal struct TimeSlotDto
{
	public string Id { get; set; }
	public TimeOnly StartTime { get; set; }
	public TimeOnly EndTime { get; set; }
	public bool IsFree { get; set; }

}