using FluentValidation;
using MusicDistribution.Application.DTOs;

namespace MusicDistribution.Application.Validators;

public class DistributeTrackRequestValidator : AbstractValidator<DistributeTrackRequest>
{
    public DistributeTrackRequestValidator()
    {
        RuleFor(x => x.DspIds)
            .NotEmpty()
            .WithMessage("At least one DSP id is required.");
        RuleForEach(x => x.DspIds).GreaterThan(0);
    }
}
