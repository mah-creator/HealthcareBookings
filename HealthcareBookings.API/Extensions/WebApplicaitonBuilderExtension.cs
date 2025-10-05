using Microsoft.OpenApi.Models;
using FluentValidation;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using MicroElements.OpenApi.FluentValidation;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net;

namespace Resturants.API.Extensions;

public static class WebApplicationBuilderExtension
{
	public static void AddPresentation(this WebApplicationBuilder builder)
	{
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = Dns.GetHostName(),
				ValidAudience = builder.Configuration["Jwt:Audience"],
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
			};
		});


		builder.Services.AddCors(c =>
			c.AddPolicy("Development", options =>
				options
					.AllowAnyOrigin()
					.AllowAnyHeader()
					.AllowAnyMethod()));

		builder.Services.AddControllers(o => o.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>())
			.AddJsonOptions(o =>
			{
				o.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
			});

		builder.Services.AddEndpointsApiExplorer();

		builder.Services.AddFluentValidationRulesToSwagger(configure =>
			configure.ValidatorSearch = new ValidatorSearchSettings
			{
				IsOneValidatorForType = false
			});

		builder.Services.AddSwaggerGen(c =>
		{
			c.SchemaFilter<FluentValidationSchemaFilter>();
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
