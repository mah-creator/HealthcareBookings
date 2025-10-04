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
[Route("/api/locations")]
[Authorize(Roles = UserRoles.Patient)]
public class PatientLocationsController(CurrentUserEntityService currentUser, IAppDbContext dbContext) : ControllerBase
{
	[HttpGet]
	public async Task<Results<Ok<IEnumerable<LocationDto>>, BadRequest<ProblemDetails>>> GetPatientLocaitons()
	{
		var patientLocations = currentUser.GetCurrentPatient().Result?.PatientProperties?.Locations;

		if (patientLocations == null)
			throw new InvalidHttpActionException("Your patient profile wasn't setup correctly");

		return TypedResults.Ok(patientLocations.Select(l => new LocationDto(l.Location)
		{
			LocationId = l.ID,
			LocationName = l.LocationName,
			IsPrimary = l.IsPrimary,
		}));
	}
	[HttpPost]
	public async Task<Results<Ok<IEnumerable<LocationDto>>, BadRequest<ProblemDetails>>> AddPatientLocation([FromBody] LocationRequest location)
	{
		var patient = await currentUser.GetCurrentPatient();

		if (patient.PatientProperties?.Locations == null)
			throw new InvalidHttpActionException("Your patient profile wasn't setup correctly");

		if (!isValidLocaiton(location, out string? message))
			throw new InvalidHttpActionException(message!);

		patient.PatientProperties.Locations.Add(new PatientLocation
		{
			PatientUID = patient.Id,
			Location = new Location
			{
				Longitude = location.longitude!.Value,
				Latitude = location.latitude!.Value,
			},
			LocationName = location.loactionName
		});

		await dbContext.SaveChangesAsync();

		return TypedResults.Ok(
			patient.PatientProperties.Locations
			.Select(l => new LocationDto(l.Location)
			{
				LocationId = l.ID,
				LocationName = l.LocationName,
				IsPrimary = l.IsPrimary
			})
		);
	}

	[HttpPatch("{locationId}")]
	public async Task<Results<Ok<LocationDto>, BadRequest<ProblemDetails>>> EditPatientLocation(string locationId, [FromBody] LocationRequest _location)
	{
		var patient = await currentUser.GetCurrentPatient();
		var locaiton = patient?.PatientProperties?.Locations?.Where(l => l.ID == locationId).FirstOrDefault();

		if (locaiton == null)
			throw new InvalidHttpActionException("The location you asked for was not found");

		if (!string.IsNullOrEmpty(_location?.loactionName))
			locaiton.LocationName = _location.loactionName;

		if (_location?.longitude != null)
		{
			if (!isValidLong(_location.longitude.Value, out string message))
				throw new InvalidHttpActionException(message);

			locaiton.Location.Longitude = _location.longitude.Value;
		}
		if (_location?.latitude != null)
		{
			if (!isValidLat(_location.latitude.Value, out string message))
				throw new InvalidHttpActionException(message);

			locaiton.Location.Latitude = _location.latitude.Value;
		}

		await dbContext.SaveChangesAsync();

		return TypedResults.Ok(
			new LocationDto(locaiton.Location)
			{
				LocationId = locaiton.ID,
				LocationName = locaiton.LocationName,
				IsPrimary = locaiton.IsPrimary
			}
		);
	}

	[HttpDelete("{locationId}")]
	public async Task<Results<Ok<string>, BadRequest<ProblemDetails>>> DeletePatientLocation(string locationId)
	{
		var patient = await currentUser.GetCurrentPatient();
		var location = patient?.PatientProperties?.Locations?.Where(l => l.ID == locationId).FirstOrDefault();

		if (location == null)
			throw new InvalidHttpActionException("The location wasn't found among you locations");

		patient.PatientProperties.Locations.Remove(location);

		await dbContext.SaveChangesAsync();

		return TypedResults.Ok($"Location '{location?.Location?.AddressText?? ""}' was deleted successfully");
	}

	[HttpPost("favor/{locationId}")]
	public async Task<Results<Ok<IEnumerable<LocationDto>>, BadRequest<ProblemDetails>>> FavorLocation(string locationId)
	{
		var patientLocations = currentUser.GetCurrentPatient().Result?.PatientProperties?.Locations 
			?? throw new InvalidHttpActionException("No locations were found for you");

		var location = patientLocations.Where(l => l.ID == locationId).FirstOrDefault() 
			?? throw new InvalidHttpActionException("The location you asked for wasn't found");

		var currentFavotire = patientLocations.Where(l => l.IsPrimary == true).FirstOrDefault();
		
		if (currentFavotire is not null)
			currentFavotire.IsPrimary = false;

		location.IsPrimary = true;

		await dbContext.SaveChangesAsync();

		return TypedResults.Ok(patientLocations.Select(l => new LocationDto(l.Location)
		{
			LocationId = l.ID,
			LocationName = l.LocationName,
			IsPrimary = l.IsPrimary
		}));
	}
	static bool isValidLong(float longitude, out string? message)
	{
		message = null;

		if (longitude > ValidationConstants.LongitudeRange.Max || longitude < ValidationConstants.LongitudeRange.Min)
		{
			message =
				$"Invalid Longitude value, valid range: "
				+ $"[{ValidationConstants.LongitudeRange.Min},{ValidationConstants.LongitudeRange.Max}]";
			return false;
		}
		return true;
	}

	static bool isValidLat(float latitude, out string? message)
	{
		message = null;

		if (latitude > ValidationConstants.LatitudeRange.Max || latitude < ValidationConstants.LatitudeRange.Min)
		{
			message =
				$"Invalid latitude value, valid range: "
				+ $"[{ValidationConstants.LatitudeRange.Min},{ValidationConstants.LatitudeRange.Max}]";
			return false;
		}
		return true;
	}

	static bool isValidLocaiton(LocationRequest locationRequest, out string? message)
	{
		(bool isValid, string? msg) result = locationRequest switch
		{
			null => (false, "Invalid location"),
			{ loactionName: null or "" }
				=> (false, "Locaiton name is required"),
			{ latitude: null } 
				=> (false, "Latitude is required"),
			{ longitude: null }
				=> (false, "Longitude is required"),
			{ latitude: not null } when !isValidLat(locationRequest.latitude.Value, out string m_lat) 
				=> (false, m_lat),
			{ longitude: not null } when !isValidLong(locationRequest.longitude.Value, out string m_long)
				=> (false, m_long),
			_ => (true, null),
		};

		message = result.msg;
		return result.isValid;
	}
}

public record LocationRequest(string? loactionName, float? longitude, float? latitude);
