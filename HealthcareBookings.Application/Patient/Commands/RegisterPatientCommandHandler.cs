using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace HealthcareBookings.Application.Patient.Commands;

public class RegisterPatientCommandHandler(UserManager<User> userManager,
	IUserStore<User> userStore) : IRequestHandler<RegisterPatientCommand>
{
	public async Task Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
	{
		var passwordValidator = userManager.PasswordValidators.First();
		var passwordHasher = userManager.PasswordHasher;

		var dbUser = await userManager.FindByEmailAsync(request.Email);

		if (dbUser is not null)
		{
			throw new ValidationException(
			[
				new ()
				{
					Code = "DuplicateUserName",
					Description = $"Username '{request.Email}' is already taken."
				}
			]);
		}

		var passwordValidationResult = await passwordValidator.ValidateAsync(userManager, dbUser!, request.Password);
		if (passwordValidationResult.Succeeded is not true)
		{
			throw new ValidationException(passwordValidationResult.Errors);
		}

		var newUser = new User
		{
			Email = request.Email,
			NormalizedEmail = request.Email.ToUpper(),
			UserName = request.Email,
			NormalizedUserName = request.Email.ToUpper()
		};
		newUser.PasswordHash = passwordHasher.HashPassword(newUser, request.Password);

		await userStore.CreateAsync(newUser, cancellationToken);
		await userManager.AddToRoleAsync(newUser, UserRoles.Patient);

	}
}
