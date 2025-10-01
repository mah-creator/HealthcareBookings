using FluentValidation;
using FluentValidation.Validators;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class FluentValidationSchemaFilter : ISchemaFilter
{
	private readonly IServiceProvider _serviceProvider;

	public FluentValidationSchemaFilter(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public void Apply(OpenApiSchema schema, SchemaFilterContext context)
	{
		using var scope = _serviceProvider.CreateScope();
		var validatorType = typeof(IValidator<>).MakeGenericType(context.Type);
		var validator = scope.ServiceProvider.GetService(validatorType) as IValidator;

		if (validator == null) return;

		var descriptor = validator.CreateDescriptor();

		foreach (var key in schema.Properties.Keys)
		{
			var rules = descriptor.GetRulesForMember(key);
			foreach (var rule in rules)
			{
				foreach (var component in rule.Components)
				{
					var v = component.Validator;

					switch (v)
					{
						case INotEmptyValidator:
							if (!schema.Required.Contains(key))
								schema.Required.Add(key);
							break;

						case ILengthValidator length:
							schema.Properties[key].MinLength = length.Min > 0 ? length.Min : null;
							schema.Properties[key].MaxLength = length.Max != int.MaxValue ? length.Max : null;
							break;

						case IGreaterThanOrEqualValidator gte:
							schema.Properties[key].Minimum = Convert.ToDecimal(gte.ValueToCompare);
							break;

						case ILessThanOrEqualValidator lte:
							schema.Properties[key].Maximum = Convert.ToDecimal(lte.ValueToCompare);
							break;
					}
				}
			}
		}
	}
}
