using FluentValidation;
using MusicDistribution.Application.DTOs;
using MusicDistribution.Domain.Enums;

namespace MusicDistribution.Application.Validators;

public class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequest>
{
    public CreateTrackRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ArtistId).GreaterThan(0);
        RuleFor(x => x.Isrc)
            .NotEmpty()
            .MaximumLength(12)
            .Matches("^[A-Za-z]{2}[A-Za-z0-9]{3}[0-9]{4,7}$")
            .WithMessage("ISRC must start with 2 country letters followed by alphanumeric characters (e.g. USRC17607839).");
        RuleFor(x => x.Genre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ReleaseDate).NotEmpty();

        When(x => !string.IsNullOrWhiteSpace(x.Status), () =>
        {
            RuleFor(x => x.Status!)
                .Must(status => Enum.TryParse<TrackStatus>(status, true, out _))
                .WithMessage("Status must be Draft, Submitted, or Distributed.");
        });
    }
}
