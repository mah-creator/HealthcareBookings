using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace HealthcareBookings.Application.Users;

public class UserRegistrationService(UserManager<User> userManager,
	IUserStore<User> userStore)
{
	public async Task<User> RegisterUser(string email, string password, string role, CancellationToken cancellationToken = default)
	{
		var passwordValidator = userManager.PasswordValidators.First();
		var passwordHasher = userManager.PasswordHasher;

		var dbUser = await userManager.FindByEmailAsync(email);

		if (dbUser is not null)
		{
			throw new InvalidHttpActionException($"Username '{email}' is already taken.");
		}

		var passwordValidationResult = await passwordValidator.ValidateAsync(userManager, dbUser!, password);
		if (passwordValidationResult.Succeeded is not true)
		{
			throw new ValidationException(passwordValidationResult.Errors);
		}

		var newUser = new User
		{
			Email = email,
			NormalizedEmail = email.ToUpper(),
			UserName = email,
			NormalizedUserName = email.ToUpper()
		};
		newUser.PasswordHash = passwordHasher.HashPassword(newUser, password);

		await userStore.CreateAsync(newUser, cancellationToken);
		await userManager.AddToRoleAsync(newUser, role);

		return newUser;
	}
}
