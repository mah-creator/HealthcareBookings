namespace HealthcareBookings.Application.Utils;

public static class ScheduleUtils
{
	public static bool IsOverlap(TimeOnly start1, TimeOnly end1, TimeOnly start2, TimeOnly end2) =>
		start1 < end2 && end1 > start2;
}
