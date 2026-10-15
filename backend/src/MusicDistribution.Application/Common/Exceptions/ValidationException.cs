namespace MusicDistribution.Application.Common.Exceptions;

/// <summary>
/// Raised when FluentValidation finds one or more rule violations.
/// Carries a per-field error map consumed by the API exception handler.
/// </summary>
public class ValidationException(IDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}
