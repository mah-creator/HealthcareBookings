using Microsoft.OpenApi.Models;

namespace Resturants.API.Extensions;

public static class WebApplicationBuilderExtension
{
    public static void AddPresentation(this WebApplicationBuilder builder)
    {
	   builder.Services.AddHttpContextAccessor();
	   builder.Services.AddAuthentication();

	   builder.Services.AddControllers();

	   builder.Services.AddEndpointsApiExplorer();

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
