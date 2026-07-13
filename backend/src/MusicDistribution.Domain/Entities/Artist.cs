namespace MusicDistribution.Domain.Entities;

/// <summary>
/// A music artist who owns one or more tracks.
/// </summary>
public class Artist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}
