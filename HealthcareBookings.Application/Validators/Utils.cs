namespace HealthcareBookings.Application.Validators;

public static class Utils
{
	public static bool IsNormalAge(DateOnly dob) => dob.CompareTo(DateOnly.FromDateTime(DateTime.Today)) < 0
									  && dob.CompareTo(new DateOnly(1900, 1, 1)) > 0;
}
