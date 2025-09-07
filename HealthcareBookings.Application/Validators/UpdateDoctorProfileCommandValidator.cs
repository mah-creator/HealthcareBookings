using FluentValidation;
using HealthcareBookings.Application.Doctors.Commands.Profile;
using HealthcareBookings.Domain.Entities;

namespace HealthcareBookings.Application.Validators;

public class UpdateDoctorProfileCommandValidator : AbstractValidator<UpdateDoctorProfileCommand>
{
	public UpdateDoctorProfileCommandValidator()
	{
		RuleFor(o => o.Gender)
			.Must(g => g.ToUpper().Equals(Gender.NormalizedFemale)
					|| g.ToUpper().Equals(Gender.NormalizedMale))
				.WithMessage($"Incorrect gender")
				.When(g => g.Gender is not null);
	}
}
