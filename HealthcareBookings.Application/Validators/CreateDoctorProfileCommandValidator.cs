using FluentValidation;
using HealthcareBookings.Application.Doctors.Commands.Profile;
using HealthcareBookings.Domain.Entities;

namespace HealthcareBookings.Application.Validators;

public class CreateDoctorProfileCommandValidator : AbstractValidator<CreateDoctorProfileCommand>
{
	public CreateDoctorProfileCommandValidator()
	{
		RuleFor(o => o.Gender)
			.NotEmpty().WithMessage("Gender is required");
		RuleFor(o => o.Name)
			.NotEmpty().WithMessage("Name is required");
		RuleFor(o => o.DateOfBirth)
			.NotEmpty().WithMessage("Date of birth is required");
		RuleFor(o => o.Bio)
			.NotEmpty().WithMessage("Bio is required");

		RuleFor(o => o.Gender)
			.Must(g => g.ToUpper().Equals(Gender.NormalizedFemale)
					|| g.ToUpper().Equals(Gender.NormalizedMale))
				.WithMessage($"Incorrect gender")
				.When(g => g.Gender is not null);
	}
}
