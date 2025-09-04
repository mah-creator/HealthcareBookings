﻿// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using HealthcareBookings.Application.Users;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HealthcareBookings.Application.CustomIdentity;

/// <summary>
/// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to add identity endpoints.
/// </summary>
public static class IdentityApiEndpointRouteBuilderExtensions
{
	// Validate the email address using DataAnnotations like the UserValidator does when RequireUniqueEmail = true.
	private static readonly EmailAddressAttribute _emailAddressAttribute = new();

	/// <summary>
	/// Add endpoints for registering, logging in, and logging out using ASP.NET Core Identity.
	/// </summary>
	/// <typeparam name="TUser">The type describing the user. This should match the generic parameter in <see cref="UserManager{TUser}"/>.</typeparam>
	/// <param name="endpoints">
	/// The <see cref="IEndpointRouteBuilder"/> to add the identity endpoints to.
	/// Call <see cref="EndpointRouteBuilderExtensions.MapGroup(IEndpointRouteBuilder, string)"/> to add a prefix to all the endpoints.
	/// </param>
	/// <returns>An <see cref="IEndpointConventionBuilder"/> to further customize the added endpoints.</returns>
	public static IEndpointConventionBuilder MapCustomIdentityApi<TUser>(this IEndpointRouteBuilder endpoints)
		where TUser : class, new()
	{
		ArgumentNullException.ThrowIfNull(endpoints);

		var timeProvider = endpoints.ServiceProvider.GetRequiredService<TimeProvider>();
		var bearerTokenOptions = endpoints.ServiceProvider.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>();
		var emailSender = endpoints.ServiceProvider.GetRequiredService<IEmailSender<TUser>>();
		var linkGenerator = endpoints.ServiceProvider.GetRequiredService<LinkGenerator>();

		// We'll figure out a unique endpoint name based on the final route pattern during endpoint generation.
		string? confirmEmailEndpointName = null;

		var routeGroup = endpoints.MapGroup("");

		// NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
		// https://github.com/dotnet/aspnetcore/issues/47338

		routeGroup.MapPost("/login", async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>>
			([FromBody] LoginRequest login, [FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies, [FromServices] IServiceProvider sp) =>
		{
			var signInManager = sp.GetRequiredService<SignInManager<TUser>>();

			var useCookieScheme = useCookies == true || useSessionCookies == true;
			var isPersistent = useCookies == true && useSessionCookies != true;
			signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

			var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent, lockoutOnFailure: true);

			if (result.RequiresTwoFactor)
			{
				if (!string.IsNullOrEmpty(login.TwoFactorCode))
				{
					result = await signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent, rememberClient: isPersistent);
				}
				else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
				{
					result = await signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
				}
			}

			if (!result.Succeeded)
			{
				return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
			}

			// The signInManager already produced the needed response in the form of a cookie or bearer token.
			return TypedResults.Empty;
		});

		//routeGroup.MapPost("/forgotPassword", async Task<Results<Ok, ValidationProblem>>
		//	([FromBody] ForgotPasswordRequest resetRequest, [FromServices] IServiceProvider sp) =>
		//{
		//	var userManager = sp.GetRequiredService<UserManager<TUser>>();
		//	var user = await userManager.FindByEmailAsync(resetRequest.Email);

		//	if (user is not null && await userManager.IsEmailConfirmedAsync(user))
		//	{
		//		var code = await userManager.GeneratePasswordResetTokenAsync(user);
		//		code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

		//		await emailSender.SendPasswordResetCodeAsync(user, resetRequest.Email, HtmlEncoder.Default.Encode(code));
		//	}

		//	// Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
		//	// returned a 400 for an invalid code given a valid user email.
		//	return TypedResults.Ok();
		//});

		//routeGroup.MapPost("/resetPassword", async Task<Results<Ok, ValidationProblem>>
		//	([FromBody] ResetPasswordRequest resetRequest, [FromServices] IServiceProvider sp) =>
		//{
		//	var userManager = sp.GetRequiredService<UserManager<TUser>>();

		//	var user = await userManager.FindByEmailAsync(resetRequest.Email);

		//	if (user is null || !(await userManager.IsEmailConfirmedAsync(user)))
		//	{
		//		// Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
		//		// returned a 400 for an invalid code given a valid user email.
		//		return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken()));
		//	}

		//	IdentityResult result;
		//	try
		//	{
		//		var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
		//		result = await userManager.ResetPasswordAsync(user, code, resetRequest.NewPassword);
		//	}
		//	catch (FormatException)
		//	{
		//		result = IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
		//	}

		//	if (!result.Succeeded)
		//	{
		//		return CreateValidationProblem(result);
		//	}

		//	return TypedResults.Ok();
		//});

		var accountGroup = routeGroup.MapGroup("/manage").RequireAuthorization();

		accountGroup.MapGet("/info", async Task<Results<Ok<CurrentUser>, ValidationProblem, NotFound>>
			(ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
		{
			var currentUserService = sp.GetRequiredService<CurrentUserService>();
			var currentUser = currentUserService.GetCurrentUser();
			if (currentUser.Id is null)
			{
				return TypedResults.NotFound();
			}

			return TypedResults.Ok(currentUser);
		}).RequireAuthorization();

		async Task SendConfirmationEmailAsync(TUser user, UserManager<TUser> userManager, HttpContext context, string email, bool isChange = false)
		{
			if (confirmEmailEndpointName is null)
			{
				throw new NotSupportedException("No email confirmation endpoint was registered!");
			}

			var code = isChange
				? await userManager.GenerateChangeEmailTokenAsync(user, email)
				: await userManager.GenerateEmailConfirmationTokenAsync(user);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

			var userId = await userManager.GetUserIdAsync(user);
			var routeValues = new RouteValueDictionary()
			{
				["userId"] = userId,
				["code"] = code,
			};

			if (isChange)
			{
				// This is validated by the /confirmEmail endpoint on change.
				routeValues.Add("changedEmail", email);
			}

			var confirmEmailUrl = linkGenerator.GetUriByName(context, confirmEmailEndpointName, routeValues)
				?? throw new NotSupportedException($"Could not find endpoint named '{confirmEmailEndpointName}'.");

			await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
		}

		return new IdentityEndpointsConventionBuilder(routeGroup);
	}

	private static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription) =>
		TypedResults.ValidationProblem(new Dictionary<string, string[]> {
			{ errorCode, [errorDescription] }
		});

	private static ValidationProblem CreateValidationProblem(IdentityResult result)
	{
		// We expect a single error code and description in the normal case.
		// This could be golfed with GroupBy and ToDictionary, but perf! :P
		Debug.Assert(!result.Succeeded);
		var errorDictionary = new Dictionary<string, string[]>(1);

		foreach (var error in result.Errors)
		{
			string[] newDescriptions;

			if (errorDictionary.TryGetValue(error.Code, out var descriptions))
			{
				newDescriptions = new string[descriptions.Length + 1];
				Array.Copy(descriptions, newDescriptions, descriptions.Length);
				newDescriptions[descriptions.Length] = error.Description;
			}
			else
			{
				newDescriptions = [error.Description];
			}

			errorDictionary[error.Code] = newDescriptions;
		}

		return TypedResults.ValidationProblem(errorDictionary);
	}

	private static async Task<InfoResponse> CreateInfoResponseAsync<TUser>(TUser user, UserManager<TUser> userManager)
		where TUser : class
	{
		return new()
		{
			Email = await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
			IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
		};
	}

	// Wrap RouteGroupBuilder with a non-public type to avoid a potential future behavioral breaking change.
	private sealed class IdentityEndpointsConventionBuilder(RouteGroupBuilder inner) : IEndpointConventionBuilder
	{
		private IEndpointConventionBuilder InnerAsConventionBuilder => inner;

		public void Add(Action<EndpointBuilder> convention) => InnerAsConventionBuilder.Add(convention);
		public void Finally(Action<EndpointBuilder> finallyConvention) => InnerAsConventionBuilder.Finally(finallyConvention);
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	private sealed class FromBodyAttribute : Attribute, IFromBodyMetadata
	{
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	private sealed class FromServicesAttribute : Attribute, IFromServiceMetadata
	{
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	private sealed class FromQueryAttribute : Attribute, IFromQueryMetadata
	{
		public string? Name => null;
	}
}