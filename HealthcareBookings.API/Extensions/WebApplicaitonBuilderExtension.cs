using Microsoft.OpenApi.Models;
using FluentValidation;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using MicroElements.OpenApi.FluentValidation;
using System.Text.Json.Serialization;

namespace Resturants.API.Extensions;

public static class WebApplicationBuilderExtension
{
	public static void AddPresentation(this WebApplicationBuilder builder)
	{
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddAuthentication();

		builder.Services.AddControllers()
			.AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

		builder.Services.AddEndpointsApiExplorer();

		builder.Services.AddFluentValidationRulesToSwagger(configure =>
			configure.ValidatorSearch = new ValidatorSearchSettings
			{
				IsOneValidatorForType = false
			});

		builder.Services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo { Title = "Test", Version = "v1" });
			c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
			{
				Type = SecuritySchemeType.Http,
				Scheme = "Bearer"
			});
			c.AddSecurityRequirement(new OpenApiSecurityRequirement
		   {
			 {
				new OpenApiSecurityScheme
				{
					Reference = new OpenApiReference{Type = ReferenceType.SecurityScheme, Id = "bearerAuth"}
				},
				[]
			 }
		   });
		});
	}
}
