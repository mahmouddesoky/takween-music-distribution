using FluentValidation;
using MusicDistribution.Application.DTOs;

namespace MusicDistribution.Application.Validators;

public class CreateArtistRequestValidator : AbstractValidator<CreateArtistRequest>
{
    public CreateArtistRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}
