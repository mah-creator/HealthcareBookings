using HealthcareBookings.Application.Constants;
using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Paging;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Domain.Exceptions;
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
	[ProducesResponseType(typeof(string), 400)]
	[ProducesResponseType(200)]
	public async Task<IActionResult> AddReview(string appointmentId, string reviewText, float rating = 0)
	{
		if (rating > 5)
			throw new InvalidHttpActionException("Rating should be in the range 1-5");

		var appointment = dbContext.Appointments.Include(a => a.Review).Include(a => a.Doctor)
			.Where(a => a.AppointmentID == appointmentId).FirstOrDefault();

		if (appointment == null)
			throw new InvalidHttpActionException("Appointment doesn't exist");
		if (appointment.Review != null)
			throw new InvalidHttpActionException("Appointment was already reviewed");

		appointment.Review = new AppointmentReview
		{
			AppointmentID = appointment.AppointmentID,
			Rating = rating,
			ReviewText = reviewText,
		};
		appointment.Doctor.Rating = (appointment.Doctor.Rating * appointment.Doctor.RatingCount + rating) / (appointment.Doctor.RatingCount + 1);
		appointment.Doctor.RatingCount+=1;
		await dbContext.SaveChangesAsync();

		return Ok(new { title = "Thank you for your review" });
	}
	
	[HttpGet("{doctorId}")]
	[ProducesResponseType(typeof(PagedList<ReviewDto>), 200)]
	public async Task<IActionResult> GetReviews(string doctorId, [Required] int page = 1, [Required] int pageSize = 10)
	{
		var reviews = dbContext.Appointments?
			.Include(a => a.Patient).ThenInclude(p => p.Account).ThenInclude(a => a.Profile)
			.Where(a => a.DoctorID == doctorId)
			.Where(a => a.Review != null)
			.Select(a => a.Review)
			.Select(r => new ReviewDto
			{
				PatientName = r.Appointment.Patient.Account.Profile.Name,
				PatientImagePath = ApiSettings.BaseUrl + r.Appointment.Patient.Account.Profile.ProfileImagePath,
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