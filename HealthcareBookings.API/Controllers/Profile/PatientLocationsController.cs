using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Patients.Queries;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareBookings.API.Controllers.Profile;

[Controller]
[Route("/api/profile/patient/locations")]
[Authorize(Roles = UserRoles.Patient)]
public class PatientLocationsController(CurrentUserEntityService currentUser, IAppDbContext dbContext) : ControllerBase
{
	[HttpPost]
	public async Task<Results<Ok<IEnumerable<LocationDto>>, BadRequest<ProblemDetails>>> AddPatientLocation([FromBody] LocationRequest location)
	{
		var patient = await currentUser.GetCurrentPatient();

		if (patient.PatientProperties?.Locations == null) 
			throw new InvalidHttpActionException("Your patient profile wasn't setup correctly");

		patient.PatientProperties.Locations.Add(new PatientLocation
		{
			PatientUID = patient.Id,
			Location = new Location
			{
				AddressText = location.loactionName,
				Longitude = location.longitude,
				Latitude = location.latitude,
			}
		});

		await dbContext.SaveChangesAsync();

		return TypedResults.Ok(
			patient.PatientProperties.Locations
			.Select(p => new LocationDto {
				LocationId = p.ID, LocationName = p.Location.AddressText, Longitude = p.Location.Longitude, Latitude = p.Location.Latitude, IsPrimary = p.IsPrimary})
		);
	}
}

public record LocationRequest(string loactionName, float longitude, float latitude);
