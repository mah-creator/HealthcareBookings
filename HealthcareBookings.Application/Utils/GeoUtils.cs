namespace HealthcareBookings.Application.Utils;

using System;

public static class GeoUtils
{
	private const double EarthRadiusKm = 6371.0; // mean Earth radius in km

	/// <summary>
	/// Calculates the great-circle distance between two points on Earth using the Haversine formula.
	/// Inputs are in DEGREES (not radians).
	/// </summary>
	public static double? DistanceKm(double? lat1Deg, double? lon1Deg, double? lat2Deg, double? lon2Deg)
	{
		if (lat1Deg == null || lon1Deg == null || lat2Deg == null || lon2Deg == null)
			return null;

		// Convert degrees to radians
		double lat1 = DegreesToRadians(lat1Deg.Value);
		double lon1 = DegreesToRadians(lon1Deg.Value);
		double lat2 = DegreesToRadians(lat2Deg.Value);
		double lon2 = DegreesToRadians(lon2Deg.Value);

		// Differences
		double dLat = (lat2 - lat1) / 2.0;
		double dLon = (lon2 - lon1) / 2.0;

		// Haversine formula
		double a = Math.Pow(Math.Sin(dLat), 2) +
				   Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon), 2);

		double c = 2 * Math.Asin(Math.Min(1.0, Math.Sqrt(a)));

		return EarthRadiusKm * c;
	}

	private static double DegreesToRadians(double degrees)
	{
		return degrees * (Math.PI / 180.0);
	}
}

