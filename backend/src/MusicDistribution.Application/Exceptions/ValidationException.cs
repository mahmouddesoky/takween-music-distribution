namespace MusicDistribution.Application.Exceptions;

/// <summary>
/// Raised when FluentValidation finds one or more rule violations.
/// Carries a per-field error map consumed by the exception middleware.
/// </summary>
public class ValidationException(IDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}
