using Z.EntityFramework.Extensions;
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
using static HealthcareBookings.Application.Utils.ScheduleUtils;
using HealthcareBookings.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

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

		var timeSlot = schedule.TimeSlots.Where(s => IsOverlap(s.StartTime, s.EndTime, start, end))?.FirstOrDefault();

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

	[HttpPost("{doctorId}/batchCreate")]
	[Authorize(Roles = UserRoles.ClinicAdmin)]
	public async Task CreateBatchSchedule(string doctorId, [FromQuery] BatchScheduleRequest request)
	{
		// Validate request
		if (!ValidateBatchScheduleRequest(request, out string? message))
			throw new InvalidHttpActionException(message!);

		// Verify doctor exists
		if (!await dbContext.Doctors.AnyAsync(d => d.DoctorUID == doctorId))
			throw new InvalidHttpActionException("Doctor wasn't found");

		// Get the range of dates once
		var start = request.FirstDay;
		var end = request.LastDay;

		// 🧩 Load ALL existing schedules for this doctor in one go
		var existingSchedules = await dbContext.DoctorSchedules
			.Include(ds => ds.TimeSlots)
			.AsNoTracking()
			.Where(ds => ds.DoctorID == doctorId && ds.Date >= start && ds.Date <= end)
			.ToListAsync();

		// Make lookup by date for faster access
		var scheduleLookup = existingSchedules.ToDictionary(ds => ds.Date, ds => ds);

		var newSchedules = new List<Schedule>();
		var newTimeSlots = new List<TimeSlot>();

		// 🔁 Iterate dates in memory
		for (var date = start; date <= end; date = date.AddDays(1))
		{
			var schedExists = scheduleLookup.TryGetValue(date, out var schedInDb);
			var schedId = schedExists ? schedInDb!.ScheduleID : Guid.NewGuid().ToString();
			var existingSlots = schedInDb?.TimeSlots ?? new List<TimeSlot>();

			// Generate the day's time slots
			var slots = new List<TimeSlot>();
			for (var time = request.StartTime; time < request.EndTime; time = time.AddMinutes(request.SlotSize))
			{
				slots.Add(new TimeSlot
				{
					ScheduleID = schedId,
					StartTime = time,
					EndTime = time.AddMinutes(request.SlotSize)
				});
			}

			// Filter out overlapping ones
			var nonOverlapping = slots
				.Where(ts1 => !existingSlots.Any(ts2 => IsOverlap(ts2.StartTime, ts2.EndTime, ts1.StartTime, ts1.EndTime)))
				.ToList();

			newTimeSlots.AddRange(nonOverlapping);

			// Add schedule if it doesn’t exist
			if (!schedExists)
			{
				newSchedules.Add(new Schedule
				{
					ScheduleID = schedId,
					DoctorID = doctorId,
					Date = date
				});
			}
		}

		// ⚙️ Persist
		if (newSchedules.Count > 0)
			dbContext.BulkInsert2(newSchedules);

		if (newTimeSlots.Count > 0)
			dbContext.BulkInsert2(newTimeSlots);
	}


	//[HttpPost("{doctorId}/batch-schedule")]
	////[Authorize(Roles = UserRoles.ClinicAdmin)]
	//public async Task CreateBatchSchedule(
	//	string doctorId,
	//	[FromQuery] BatchScheduleRequest request)
	//{
	//	var requestValidatioResult = ValidateBatchScheduleRequest(request, out string? message);
	//	if (requestValidatioResult is false)
	//		throw new InvalidHttpActionException(message!);

	//	if (!dbContext.Doctors.Any(d => d.DoctorUID == doctorId))
	//		throw new InvalidHttpActionException("Doctor wasn't found");

	//	List<Schedule> schedules = [];
	//	List<TimeSlot> timeSlots = [];
	//	for (DateOnly date = request.FirstDay; date <= request.LastDay; date = date.AddDays(1))
	//	{
	//		List<TimeSlot> slots = [];
	//		var schedInDb = dbContext.DoctorSchedules.Include(ds => ds.TimeSlots)
	//			.AsNoTracking()
	//			.Where(ds => ds.Date == date).FirstOrDefault();
	//		var schedSlots = schedInDb?.TimeSlots;

	//		(string schedId, bool schedExists) = schedInDb != null? (schedInDb.ScheduleID, true) : (Guid.NewGuid().ToString(), false);

	//		for (var time = request.StartTime; time < request.EndTime; time = time.AddMinutes(request.SlotSize))
	//		{
	//			slots.Add(new TimeSlot
	//			{
	//				ScheduleID = schedId,
	//				StartTime = time,
	//				EndTime = time.AddMinutes(request.SlotSize)
	//			});
	//		}

	//		// if sched is new or has no time slots, keep all generated time slots
	//		if (schedSlots is null)
	//			timeSlots.AddRange(slots);
	//		// otherwise filter out slots that overlap with existing slots
	//		else
	//			timeSlots.AddRange(slots.Where(ts1 =>
	//				!schedSlots.Any(ts2 => IsOverlap(ts2.StartTime, ts2.EndTime, ts1.StartTime, ts1.EndTime))));

	//		// if schedule doesn't exist, create a db entry for it
	//		if (!schedExists)
	//		{
	//			schedules.Add(new Schedule
	//			{
	//				ScheduleID = schedId,
	//				DoctorID = doctorId,
	//				Date = date
	//			});
	//		}
	//	}

	//	dbContext.DoctorSchedules.AddRange(schedules);

	//	dbContext.BulkInsert2(schedules);

	//	dbContext.DoctorTimeSlots.AddRange(timeSlots);

	//	dbContext.BulkInsert2(timeSlots);
	//}
	static bool ValidateBatchScheduleRequest(BatchScheduleRequest? request, out string? message)
	{
		(bool isValid, string? msg) result = request switch
		{
			null
				=> (false, "Invalid schedule request"),

			_ when request.FirstDay == default
				=> (false, "First day is required"),

			_ when request.LastDay == default
				=> (false, "Last day is required"),

			_ when request.LastDay < request.FirstDay
				=> (false, "Last day cannot be before first day"),

			_ when request.LastDay > request.FirstDay.AddMonths(1)
				=> (false, "The schedule range must not exceed 3 months"),

			_ when request.WorkDays == null || request.WorkDays.Count == 0
				=> (false, "At least one work day must be provided"),

			_ when request.StartTime == default
				=> (false, "Start time is required"),

			_ when request.EndTime == default
				=> (false, "End time is required"),

			_ when request.StartTime - request.EndTime > TimeSpan.FromHours(8)
				=> (false, "Working hours can't exceed 8 hours"),

			_ when request.EndTime <= request.StartTime
				=> (false, "End time must be after start time"),

			_ when request.SlotSize < 10 || request.SlotSize > 40
				=> (false, "Slot size must be between 10 and 40 minutes"),

			_ => (true, null)
		};

		message = result.msg;
		return result.isValid;
	}


}

public record BatchScheduleRequest
(
	DateOnly FirstDay,
	DateOnly LastDay,
	List<DayOfWeek> WorkDays,
	TimeOnly StartTime,
	TimeOnly EndTime,
	int SlotSize 
);