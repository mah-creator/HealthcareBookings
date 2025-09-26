using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Infrastructure.Extensions;
using HealthcareBookings.Application.Extensions;
using HealthcareBookings.Infrastructure.Persistence;
using HealthcareBookings.Infrastructure.Seeder;
using Microsoft.AspNetCore.Identity;
using Resturants.API.Extensions;
using HealthcareBookings.Application.Middleware;
using HealthcareBookings.Application.CustomIdentity;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.Formatters;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddInfrastucture(builder.Configuration);
builder.Services.AddApplication();
builder.AddPresentation();

builder.Services.AddIdentityApiEndpoints<User>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddOptions<BearerTokenOptions>(IdentityConstants.BearerScheme)
	.Configure(options => {
		options.BearerTokenExpiration = TimeSpan.FromDays(365);
	});

builder.Services.AddFluentValidationRulesToSwagger();


var app = builder.Build();

var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<IAppSeeder>();
await seeder.Seed();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

/*
    * Group identity endpoints provided by the Identity package under "api/identity"
    * and add them to the Identity section in swagger UI
    */
app.MapGroup("api/identity")
    .WithTags("Identity")
    .MapCustomIdentityApi<User>();

app.UseStaticFiles();

app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleawre>();

app.MapControllers();

app.Run();
