using HealthcareBookings.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace HealthcareBookings.Application.Middlewares;

public class PersistedTokenValidatorMiddleware(RequestDelegate next)
{
	public async Task Invoke(HttpContext context)
	{
		var userManager = context.RequestServices.GetRequiredService<UserManager<User>>();
		var signinManager = context.RequestServices.GetRequiredService<SignInManager<User>>();

		var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
		var tokenName = context.User.FindFirstValue("token_name");
		var user = await userManager.FindByIdAsync(userId);

		if (user != null)
		{
			if (tokenName == null)
			{
				context.Response.StatusCode = 401;
				return;
			}

			var tokenInDb = await userManager.GetAuthenticationTokenAsync(user, IdentityConstants.BearerScheme, tokenName);
			var token = await context.GetTokenAsync("access_token");
			if (tokenInDb == null || token == null || tokenInDb != token)
			{
				context.Response.StatusCode = 401;
				return;
			}

		}
		await next(context);
	}
}