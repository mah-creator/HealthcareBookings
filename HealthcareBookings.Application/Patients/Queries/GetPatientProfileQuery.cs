using HealthcareBookings.Application.Patients.Queries;
using HealthcareBookings.Domain.Entities;

namespace HealthcareBookings.Application.Patients.Queries;

public class GetPatientProfileQuery
{
	public string Name { get; set; }
	public DateOnly? DateOfBirth { get; set; }
	public string Gender { get; set; }
	public  IEnumerable<LocationDto> Locations { get; set; }
	public string ProfileImagePath { get; set; }
}


public class LocationDto : Location
{
	public LocationDto(Location l)
	{
		City = l.City;
		Country = l.Country;
		StreetNumber = l.StreetNumber;
		StreetName = l.StreetName;
		PostalCode = l.PostalCode;
		AddressText = l.AddressText;
		Latitude = l.Latitude;
		Longitude = l.Longitude;
	}
	public string LocationId { get; set; }
	public string LocationName { get; set; }
	public bool? IsPrimary { get; set; }
}