namespace MusicDistribution.Domain.Enums;

/// <summary>
/// Lifecycle of a track within the distribution pipeline.
/// </summary>
public enum TrackStatus
{
    Draft = 0,
    Submitted = 1,
    Distributed = 2
}
