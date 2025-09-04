namespace HealthcareBookings.Domain.Exceptions;

public class InvalidHttpActionException(string message) : Exception(message)
{
}
