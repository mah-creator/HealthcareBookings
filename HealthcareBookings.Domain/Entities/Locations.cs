using System.ComponentModel.DataAnnotations;

namespace HealthcareBookings.Domain.Entities;

public class PatientLocation
{
    public string ID { get; set; } = Guid.NewGuid().ToString();
    public string PatientUID { get; set; }
    public Location Location { get; set; }
    public bool IsPrimary { get; set; } = false;
}

public class Location
{
	public float Longitude { get; set; }
	public float Latitude { get; set; }
    public int? StreetNumber { get; set; }
    public string? StreetName { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string AddressText { get; set; }

	public override string ToString()
	{
        return $"{Country}, {City}";
	}
}