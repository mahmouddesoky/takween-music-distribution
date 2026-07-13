using FluentValidation;
using MusicDistribution.Application.DTOs;
using MusicDistribution.Domain.Enums;

namespace MusicDistribution.Application.Validators;

public class UpdateTrackStatusRequestValidator : AbstractValidator<UpdateTrackStatusRequest>
{
    public UpdateTrackStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => Enum.TryParse<TrackStatus>(status, true, out _))
            .WithMessage("Status must be Draft, Submitted, or Distributed.");
    }
}
