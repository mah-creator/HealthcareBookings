using HealthcareBookings.Application.Data;
using HealthcareBookings.Application.Patients.Ectensions;
using HealthcareBookings.Application.Users;
using HealthcareBookings.Domain.Constants;
using HealthcareBookings.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace HealthcareBookings.API.Controllers.Identity;

[Controller]
[Route("/api/identity")]
[Tags("Identity")]
public class CustomeIdentityEndpoints(
	IConfiguration configuration,
	SignInManager<User> signInManager,
	UserManager<User> userManager,
	IAppDbContext dbContext) : ControllerBase
{
	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequest login)
	{
		signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

		var user = await userManager.FindByEmailAsync(login.Email);

		if (user == null || !signInManager.CheckPasswordSignInAsync(user, login.Password, true).Result.Succeeded)
		{
			return Problem(title: "Failed to login", statusCode: StatusCodes.Status401Unauthorized);
		}

		var roles = await userManager.GetRolesAsync(user);

		var token = GenerateJwtToken(user, roles);

		if (roles.FirstOrDefault()?.Equals(UserRoles.Patient)?? false)
		{
			var patient = dbContext.Patients.IncludeAll().FirstOrDefault(p => p.PatientUID == user.Id);
			return Ok(new
			{
				accessToken = token,
				role = roles.FirstOrDefault(),
				profileComleted = patient?.Account?.Profile != null,
				locationsCreated =
					   patient?.Locations != null
					&& patient.Locations.Count > 0
			});
		}

		return Ok(new
		{
			accessToken = token,
			role = roles.FirstOrDefault(),
		});
	}

		 private string GenerateJwtToken(IdentityUser user, IList<string> roles)
	{
		var allClaims = new List<Claim>
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.Id),
			new Claim(JwtRegisteredClaimNames.Email, user.Email),
		};
		
		allClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: Dns.GetHostName(),
			audience: configuration["Jwt:Audience"],
			claims: allClaims,
			expires: DateTime.Now.AddDays(300), // Token expiration time
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}

public record LoginRequest(string Email, string Password);
