using MusicDistribution.Domain.Enums;

namespace MusicDistribution.Domain.Entities;

/// <summary>
/// A music track owned by an artist and distributable to one or more DSPs.
/// </summary>
public class Track
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ArtistId { get; set; }
    /// <summary>International Standard Recording Code — globally unique per recording.</summary>
    public string Isrc { get; set; } = string.Empty;
    public DateOnly ReleaseDate { get; set; }
    public string Genre { get; set; } = string.Empty;
    public TrackStatus Status { get; set; } = TrackStatus.Draft;

    public Artist Artist { get; set; } = null!;
    public ICollection<TrackDistribution> Distributions { get; set; } = new List<TrackDistribution>();
}
