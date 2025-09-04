using HealthcareBookings.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace HealthcareBookings.Application.Users;

public class CurrentUserService(
		IHttpContextAccessor httpContextAccessor,
		UserManager<User> userManager)
{
	public CurrentUser GetCurrentUser()
	{
		var user = httpContextAccessor.HttpContext!.User;
		var userId = user.FindFirst(c =>
		{
			return c.Type == ClaimTypes.NameIdentifier;
		})!.Value;

		var userEmail = user.FindFirst(c =>
		{
			return c.Type == ClaimTypes.Email;
		})!.Value;

		var userRole = user.Claims.Where(c =>
		{
			return c.Type == ClaimTypes.Role;
		}).ToList().First().Value;


		return new CurrentUser
		(
			Id : userId,
			Email : userEmail,
			Role : userRole
		);
	}
}
