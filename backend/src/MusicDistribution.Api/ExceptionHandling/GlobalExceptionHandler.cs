using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MusicDistribution.Application.Common.Exceptions;
using MusicDistribution.Domain.Exceptions;

namespace MusicDistribution.Api.ExceptionHandling;

/// <summary>
/// Translates application and domain exceptions into RFC 7807 ProblemDetails responses so the
/// API has a single, consistent error contract regardless of the failure type.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails = exception switch
        {
            ValidationException validation => new ValidationProblemDetails(validation.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = validation.Message
            },
            NotFoundException => Problem(StatusCodes.Status404NotFound, "Resource not found", exception.Message),
            ConflictException => Problem(StatusCodes.Status409Conflict, "Conflict", exception.Message),
            DomainException => Problem(StatusCodes.Status409Conflict, "Business rule violated", exception.Message),
            UnauthorizedException => Problem(StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message),
            // Never echo internal exception details to the client.
            _ => Problem(StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred.")
        };

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    private static ProblemDetails Problem(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };
}
