using HealthcareBookings.Application.Constants;
using HealthcareBookings.Domain.Entities;
using static HealthcareBookings.Application.Utils.GeoUtils;

namespace HealthcareBookings.Application.Clinics;

public static class Utils
{
	public static ClinicDto CreateClinicDto(Clinic c, User patient, float? latitude, float? longitude)
	{
		var distance = DistanceKm(c.Location.Latitude, c.Location.Longitude, ((double?)latitude), ((double?)longitude));
		return new ClinicDto
		{
			Id = c.ClinicID,
			Name = c.ClinicName,
			Image = ApiSettings.BaseUrl + c.ImagePath,
			Description = c.ClinicDescription!,
			Rating = c.Rating,
			RatingCount = c.RatingCount,
			Location = c.Location,
			IsFavorite = patient.PatientProperties.FavoriteClinics.Find(fc => fc.ClinicID == c.ClinicID) is not null,
			DestanceKm = distance,
			Distance = distance == null ? null :
				distance >= 1 ? string.Format("{0:F1} Km", distance) : string.Format("{0:D} m", (int?)(distance * 1000))
		};
	}
}

public struct ClinicDto
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string Image { get; set; }
	public string Description { get; set; }
	public float Rating { get; set; }
	public int RatingCount { get; set; }
	public bool IsFavorite { get; set; }
	public double? DestanceKm { get; set; }
	public string? Distance { get; set; }
	public Location Location { get; set; }
}
