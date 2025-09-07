using FluentValidation;
using HealthcareBookings.Application.Clinics.Commands.Profile;
using HealthcareBookings.Domain.Constants;

namespace HealthcareBookings.Application.Validators;

public class CreateClinicProfileComaandValidator : AbstractValidator<CreateClinicProfileCommand>
{
	public CreateClinicProfileComaandValidator()
	{
		RuleFor	(cp => cp.Name)
			.NotEmpty().WithMessage("Clinic name is required");
		RuleFor (cp => cp.Description)
			.NotEmpty().WithMessage("Clinic description is required");
		
		RuleFor(cp => cp.Location.Country)
			.NotNull().WithMessage("Country is required");
		RuleFor(cp => cp.Location.City)
			.NotEmpty().WithMessage("City is required");

		RuleFor(cp => cp.Location.Latitude)
			.NotNull().WithMessage("Latitude is required")
			.Must(lat => 
					lat >= ValidationConstants.LatitudeRange.Min && 
					lat <= ValidationConstants.LatitudeRange.Max)
				.When(cp => cp.Location is not null)
				.WithMessage($"Invalid latitude value, valid range: "
					+ $"[{ValidationConstants.LatitudeRange.Min},{ValidationConstants.LatitudeRange.Max}]");

		RuleFor(cp => cp.Location.Longitude)
			.NotNull().WithMessage("Longitude is required")
			.Must(lng =>
				lng >= ValidationConstants.LongitudeRange.Min &&
				lng <= ValidationConstants.LongitudeRange.Max)
			.When(cp => cp.Location is not null)
			.WithMessage($"Invalid longitude value, valid range: "
				+ $"[{ValidationConstants.LongitudeRange.Min},{ValidationConstants.LongitudeRange.Max}]");

	}
}
