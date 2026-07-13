using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MusicDistribution.Application.Exceptions;

namespace MusicDistribution.Api.Middleware;

/// <summary>
/// Translates domain exceptions into RFC 7807 ProblemDetails responses so the
/// API has a single, consistent error contract regardless of the failure type.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                validationException.Errors),
            NotFoundException => (
                StatusCodes.Status404NotFound,
                exception.Message,
                (IDictionary<string, string[]>?)null),
            ConflictException => (
                StatusCodes.Status409Conflict,
                exception.Message,
                (IDictionary<string, string[]>?)null),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                exception.Message,
                (IDictionary<string, string[]>?)null),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                (IDictionary<string, string[]>?)null)
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = errors is null ? title : null,
            Instance = context.Request.Path
        };

        if (errors is not null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
