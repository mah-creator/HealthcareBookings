using FluentValidation;
using HealthcareBookings.Application.Patients.Commands.Profile;
using HealthcareBookings.Domain.Entities;

namespace HealthcareBookings.Application.Validators;

public class CreatePatientProfileCommandValidator : AbstractValidator<CreatePatientProfileCommand>
{
	public CreatePatientProfileCommandValidator()
	{
		RuleFor(p => p.Name)
			.NotEmpty().WithMessage("Name is required");
		RuleFor(p => p.Gender)
			.NotEmpty().WithMessage("Gender is required");
		RuleFor(p => p.DateOfBirth)
			.NotNull().WithMessage("Date of birth is required");

		RuleFor(p => p.Gender)
			.Must(g => g.ToUpper().Equals(Gender.NormalizedFemale)
					|| g.ToUpper().Equals(Gender.NormalizedMale))
				.WithMessage($"Incorrect gender")
				.When(p => p.Gender is not null);

		RuleFor(p => p.DateOfBirth)
			.Must(dob => Utils.IsNormalAge(dob.Value))
			.When(p => p.DateOfBirth is not null)
			.WithMessage("Invalid date of birth");
	}
}
