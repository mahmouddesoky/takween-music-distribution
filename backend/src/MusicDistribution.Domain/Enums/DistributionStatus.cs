namespace MusicDistribution.Domain.Enums;

/// <summary>
/// Status of a single track-to-DSP distribution submission.
/// </summary>
public enum DistributionStatus
{
    Pending = 0,
    Live = 1,
    Rejected = 2
}
