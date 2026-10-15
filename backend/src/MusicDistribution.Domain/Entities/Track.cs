using MusicDistribution.Domain.Enums;
using MusicDistribution.Domain.Exceptions;

namespace MusicDistribution.Domain.Entities;

/// <summary>
/// A music track owned by an artist and distributable to one or more DSPs.
/// Status and distributions can only change through <see cref="DistributeTo"/> and
/// <see cref="ChangeStatus"/>, so every caller goes through the same rules:
/// <list type="bullet">
/// <item>A new track starts as Draft.</item>
/// <item>Draft → Submitted happens by distributing to at least one DSP.</item>
/// <item>Submitted → Distributed is allowed once the track has a DSP submission.</item>
/// <item>No backwards moves (nothing goes back to Draft, Distributed does not go back to Submitted).</item>
/// </list>
/// </summary>
public class Track
{
    private readonly List<TrackDistribution> _distributions = [];

    // For EF Core.
    private Track() { }

    public Track(string title, int artistId, string isrc, DateOnly releaseDate, string genre)
    {
        Title = title;
        ArtistId = artistId;
        Isrc = isrc;
        ReleaseDate = releaseDate;
        Genre = genre;
        Status = TrackStatus.Draft;
    }

    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ArtistId { get; set; }
    /// <summary>International Standard Recording Code — globally unique per recording.</summary>
    public string Isrc { get; set; } = string.Empty;
    public DateOnly ReleaseDate { get; set; }
    public string Genre { get; set; } = string.Empty;
    public TrackStatus Status { get; private set; } = TrackStatus.Draft;

    public Artist Artist { get; set; } = null!;
    public IReadOnlyCollection<TrackDistribution> Distributions => _distributions.AsReadOnly();

    /// <summary>
    /// Submits the track to the given DSPs. Each new submission starts as Pending,
    /// and a Draft track moves into the pipeline (Submitted).
    /// </summary>
    public void DistributeTo(IEnumerable<int> dspIds, DateTime submittedAtUtc)
    {
        var requested = dspIds.Distinct().ToList();
        if (requested.Count == 0)
        {
            throw new DomainException("At least one DSP is required to distribute a track.");
        }

        var alreadySubmitted = _distributions
            .Where(distribution => requested.Contains(distribution.DspId))
            .Select(distribution => distribution.DspId)
            .ToList();

        if (alreadySubmitted.Count > 0)
        {
            throw new DomainException(
                $"Track is already submitted to DSP id(s): {string.Join(", ", alreadySubmitted)}.");
        }

        foreach (var dspId in requested)
        {
            _distributions.Add(new TrackDistribution
            {
                TrackId = Id,
                DspId = dspId,
                SubmittedAt = submittedAtUtc,
                Status = DistributionStatus.Pending
            });
        }

        if (Status == TrackStatus.Draft)
        {
            Status = TrackStatus.Submitted;
        }
    }

    /// <summary>
    /// Moves the track forward in its lifecycle. Setting the current status again is a no-op.
    /// </summary>
    public void ChangeStatus(TrackStatus newStatus)
    {
        if (newStatus == Status)
        {
            return;
        }

        if (newStatus < Status)
        {
            throw new DomainException($"A track cannot move back from {Status} to {newStatus}.");
        }

        if (_distributions.Count == 0)
        {
            throw new DomainException(
                $"A track must be submitted to at least one DSP before it can be {newStatus}; use the distribute endpoint.");
        }

        Status = newStatus;
    }

    /// <summary>
    /// Restores a known lifecycle state for reference/sample data. Not for use-case code.
    /// </summary>
    public static Track Seeded(int id, string title, int artistId, string isrc, DateOnly releaseDate, string genre, TrackStatus status) =>
        new(title, artistId, isrc, releaseDate, genre) { Id = id, Status = status };
}
