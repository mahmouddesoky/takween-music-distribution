namespace MusicDistribution.Domain.Entities;

/// <summary>
/// A Digital Service Provider (DSP) such as Spotify, Apple Music, YouTube Music.
/// </summary>
public class Dsp
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<TrackDistribution> Distributions { get; set; } = new List<TrackDistribution>();
}
