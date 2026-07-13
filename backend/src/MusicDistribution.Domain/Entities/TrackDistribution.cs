using MusicDistribution.Domain.Enums;

namespace MusicDistribution.Domain.Entities;

/// <summary>
/// Join entity representing the submission of a track to a specific DSP.
/// </summary>
public class TrackDistribution
{
    public int Id { get; set; }
    public int TrackId { get; set; }
    public int DspId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DistributionStatus Status { get; set; } = DistributionStatus.Pending;

    public Track Track { get; set; } = null!;
    public Dsp Dsp { get; set; } = null!;
}
