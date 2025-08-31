using Microsoft.AspNetCore.Identity;

namespace HealthcareBookings.Domain.Exceptions;

public class UserValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    public UserValidationException(IEnumerable<IdentityError> errors)
    {
		foreach (var item in errors)
		{
			Errors.Add(item.Code, [item.Description]);
		}
	}
}
