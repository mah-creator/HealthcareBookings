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
		var patient = currentUser.GetCurrentPatient().Result?.PatientProperties;

		if (patient == null)
			throw new InvalidHttpActionException("Your patient profile wasn't setup correctly");

		if (!isValidLocaiton(location, out string? message))
			throw new InvalidHttpActionException(message!);

		var newLocation = new PatientLocation
		{
			LocationName = location.LocationName,
			PatientUID = patient.PatientUID,
			Location = location
		};

		var locations = patient.Locations;

		if (locations == null || locations.Count == 0)
			newLocation.IsPrimary = true;
		if (locations == null)
			locations = [];

		locations.Add(newLocation);

		await dbContext.SaveChangesAsync();

		return TypedResults.Ok(
			locations
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

		if (!string.IsNullOrEmpty(_location?.LocationName))
			locaiton.LocationName = _location.LocationName;

		if (_location?.Longitude != null)
		{
			if (!isValidLong(_location.Longitude.Value, out string message))
				throw new InvalidHttpActionException(message);

			locaiton.Location.Longitude = _location.Longitude.Value;
		}
		if (_location?.Latitude != null)
		{
			if (!isValidLat(_location.Latitude.Value, out string message))
				throw new InvalidHttpActionException(message);

			locaiton.Location.Latitude = _location.Latitude.Value;
		}

		if (!string.IsNullOrEmpty(_location?.City))
			locaiton.Location.City = _location.City;
		if (!string.IsNullOrEmpty(_location?.Country))
			locaiton.Location.Country = _location.Country;
		if (!string.IsNullOrEmpty(_location?.StreetName))
			locaiton.Location.StreetName = _location.StreetName;
		if (_location?.StreetNumber  != null)
			locaiton.Location.StreetNumber = _location.StreetNumber;
		if (!string.IsNullOrEmpty(_location?.PostalCode))
			locaiton.Location.PostalCode = _location.PostalCode;
		if (!string.IsNullOrEmpty(_location?.AddressText))
			locaiton.Location.AddressText = _location.AddressText;

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

		if (location.IsPrimary)
			throw new InvalidHttpActionException($"Can't remove primary location '{location.LocationName}'");

		patient.PatientProperties.Locations.Remove(location);

		await dbContext.SaveChangesAsync();

		return TypedResults.Ok($"Location '{location?.LocationName}' was deleted successfully");
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
			{ LocationName: null or "" }
				=> (false, "Locaiton name is required"),
			{ Latitude: null } 
				=> (false, "Latitude is required"),
			{ Longitude: null }
				=> (false, "Longitude is required"),
			{ Latitude: not null } when !isValidLat(locationRequest.Latitude.Value, out string m_lat) 
				=> (false, m_lat),
			{ Longitude: not null } when !isValidLong(locationRequest.Longitude.Value, out string m_long)
				=> (false, m_long),
			_ => (true, null),
		};

		message = result.msg;
		return result.isValid;
	}
}

public class LocationRequest : Location
{
	public string LocationName { get; set; }
}