using FluentValidation;
using HealthcareBookings.Application.Patients.Commands.Profile;
using HealthcareBookings.Domain.Entities;

namespace HealthcareBookings.Application.Validators;

public class UpdatePatientProfileCommandValidator : AbstractValidator<UpdatePatientProfileCommand>
{
	public UpdatePatientProfileCommandValidator()
	{
		RuleFor(p => p.DateOfBirth)
			.Must(dob => Utils.IsNormalAge(dob.Value))
			.When(p => p.DateOfBirth is not null)
			.WithMessage("Invalid date of birth");

		RuleFor(p => p.Gender)
			.Must(g => g.ToUpper().Equals(Gender.NormalizedFemale)
					|| g.ToUpper().Equals(Gender.NormalizedMale))
				.WithMessage($"Incorrect gender")
				.When(g => g.Gender is not null);
	}
}
