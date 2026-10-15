using FluentValidation;
using MusicDistribution.Domain.Enums;

namespace MusicDistribution.Application.Tracks;

internal static class TrackRules
{
    public const string StatusMessage = "Status must be Draft, Submitted, or Distributed.";

    /// <summary>Only the enum names are accepted; numeric strings such as "5" are rejected.</summary>
    public static bool IsValidStatus(string? status) =>
        !string.IsNullOrWhiteSpace(status)
        && !int.TryParse(status, out _)
        && Enum.TryParse<TrackStatus>(status.Trim(), true, out _);

    public static TrackStatus ParseStatus(string status) => Enum.Parse<TrackStatus>(status.Trim(), true);
}

public class TrackFilterRequestValidator : AbstractValidator<TrackFilterRequest>
{
    public TrackFilterRequestValidator()
    {
        RuleFor(x => x.ArtistId).GreaterThan(0).When(x => x.ArtistId.HasValue);
        RuleFor(x => x.Genre).MaximumLength(100);
        RuleFor(x => x.Status)
            .Must(TrackRules.IsValidStatus)
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage(TrackRules.StatusMessage);
    }
}

public class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequest>
{
    public CreateTrackRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ArtistId).GreaterThan(0);
        // ISRC = 2-letter country code + 3-char registrant + 2-digit year + 5-digit designation (12 chars).
        RuleFor(x => x.Isrc)
            .NotEmpty()
            .Matches("^[A-Za-z]{2}[A-Za-z0-9]{3}[0-9]{7}$")
            .WithMessage("ISRC must be 12 characters: 2 country letters, 3 alphanumeric registrant characters and 7 digits (e.g. USRC17607839).");
        RuleFor(x => x.Genre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ReleaseDate).NotEmpty();
    }
}

public class UpdateTrackStatusRequestValidator : AbstractValidator<UpdateTrackStatusRequest>
{
    public UpdateTrackStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(TrackRules.IsValidStatus)
            .WithMessage(TrackRules.StatusMessage);
    }
}

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
