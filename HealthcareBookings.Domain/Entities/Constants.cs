namespace HealthcareBookings.Domain.Entities;

public static class Gender
{
    public const string Male = "Male";
	public const string NormalizedMale = "MALE";

    public const string Female = "Female";
    public const string NormalizedFemale = "FEMALE";
}

public static class AppointmentStatus
{
    public const string Upcoming = "Upcoming";
    public const string Completed = "Completed";
    public const string Canceled = "Canceled";
}