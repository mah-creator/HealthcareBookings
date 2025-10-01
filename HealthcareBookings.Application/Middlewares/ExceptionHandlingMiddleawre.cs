using HealthcareBookings.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HealthcareBookings.Application.Middleware;

public class ExceptionHandlingMiddleawre(ILogger<ExceptionHandlingMiddleawre> logger) : IMiddleware
{
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		try
		{
			await next.Invoke(context);
		}
		catch (ValidationException e)
		{
			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			await context.Response.WriteAsJsonAsync(new ValidationProblemDetails
			{
				Errors = e.Errors
			});

		}
		catch (InvalidHttpActionException e)
		{
			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			await context.Response.WriteAsJsonAsync(new ProblemDetails
			{
				Title = e.Message
			});
		}
		catch (Exception e)
		{
			logger.LogError(e.ToString());
			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			await context.Response.WriteAsync("Internal server error");
		}
	}
}
