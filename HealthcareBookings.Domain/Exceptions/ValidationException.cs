using Microsoft.AspNetCore.Identity;

namespace HealthcareBookings.Domain.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    public ValidationException(IEnumerable<IdentityError> errors)
    {
	   foreach (var item in errors)
	   {
		   Errors.Add(item.Code, [item.Description]);
	   }
    }
}
