using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Paging;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HealthcareBookings.API.Controllers.Reviews;

[Controller]
[Route("/api/review")]
public class ReviewsController(IAppDbContext dbContext) : ControllerBase
{
	[HttpPost("{appointmentId}")]
	[Authorize(Roles = UserRoles.Patient)]
	public async Task<IActionResult> AddReview(string appointmentId, string reviewText, float rating = 0)
	{
		if (rating > 5)
			return BadRequest("Rating should be in the range 1-5");

		var appointment = dbContext.Appointments.Include(a => a.Review).Include(a => a.Doctor)
			.Where(a => a.AppointmentID == appointmentId).FirstOrDefault();

		if (appointment == null)
			return BadRequest("Appointment doesn't exist");
		if (appointment.Review != null)
			return BadRequest("Appointment was already reviewed");

		appointment.Review = new AppointmentReview
		{
			AppointmentID = appointment.AppointmentID,
			Rating = rating,
			ReviewText = reviewText,
		};
		appointment.Doctor.Rating = (appointment.Doctor.Rating * appointment.Doctor.RatingCount + rating) / (appointment.Doctor.RatingCount + 1);
		appointment.Doctor.RatingCount+=1;
		await dbContext.SaveChangesAsync();

		return Ok();
	}
	
	[HttpGet("{doctorId}")]
	public async Task<IActionResult> GetReviews(string doctorId, [Required] int page = 1, [Required] int pageSize = 10)
	{
		var reviews = dbContext.Appointments?
			.Where(a => a.DoctorID == doctorId)
			.Where(a => a.Review != null)
			.Select(a => a.Review)
			.Include(r => r.Appointment).ThenInclude(a => a.Patient).ThenInclude(p => p.Account).ThenInclude(a => a.Profile)
			.Select(r => new ReviewDto
			{
				PatientName = r.Appointment.Patient.Account.Profile.Name,
				PatientImagePath = r.Appointment.Patient.Account.Profile.ProfileImagePath,
				Rating = r.Rating,
				ReviewText = r.ReviewText
			})
			.AsQueryable();

		return Ok(PagedList<ReviewDto>.CreatePagedList(reviews, page, pageSize));
	}
}

internal class ReviewDto
{
	public string PatientName { get; set; }
	public string PatientImagePath { get; set; }
	public float Rating { get; set; }
	public string ReviewText { get; set; }
}