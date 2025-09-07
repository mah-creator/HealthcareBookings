namespace HealthcareBookings.Domain.Constants;

public static class ValidationConstants
{
	public static MyRange LatitudeRange = new MyRange(-90, 90);
	public static MyRange LongitudeRange = new MyRange(-180, 180);
}

public struct MyRange(float min, float max)
{
	public float Min = min;
	public float Max = max;
}