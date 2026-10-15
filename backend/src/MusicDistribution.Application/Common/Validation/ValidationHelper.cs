using FluentValidation;
using FluentValidation.Results;
using AppValidationException = MusicDistribution.Application.Common.Exceptions.ValidationException;

namespace MusicDistribution.Application.Common.Validation;

/// <summary>
/// Runs a FluentValidation validator and throws an application <see cref="AppValidationException"/>
/// whose error map can be serialised directly by the API exception handler.
/// </summary>
public static class ValidationHelper
{
    public static async Task ValidateAndThrowAsync<T>(
        IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        ValidationResult result = await validator.ValidateAsync(instance, cancellationToken);
        if (result.IsValid)
        {
            return;
        }

        var errors = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        throw new AppValidationException(errors);
    }
}
