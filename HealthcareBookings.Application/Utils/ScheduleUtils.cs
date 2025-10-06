namespace HealthcareBookings.Application.Utils;

public static class ScheduleUtils
{
	public static bool timeWithin(TimeOnly start, TimeOnly end, TimeOnly time) => time <= end && time >= start;
}
