namespace MusicDistribution.Application.Tracks;

public record TrackSummaryResponse(
    int Id,
    string Title,
    int ArtistId,
    string ArtistName,
    string Isrc,
    DateOnly ReleaseDate,
    string Genre,
    string Status);

public record DistributionStatusDto(
    int DspId,
    string DspName,
    string Status,
    DateTime SubmittedAt);

public record TrackDetailResponse(
    int Id,
    string Title,
    int ArtistId,
    string ArtistName,
    string Isrc,
    DateOnly ReleaseDate,
    string Genre,
    string Status,
    IReadOnlyList<DistributionStatusDto> Distributions);

/// <summary>Query-string filters for GET /api/tracks. All optional.</summary>
public record TrackFilterRequest(int? ArtistId = null, string? Genre = null, string? Status = null);

public record CreateTrackRequest(
    string Title,
    int ArtistId,
    string Isrc,
    DateOnly ReleaseDate,
    string Genre);

public record UpdateTrackStatusRequest(string Status);

public record DistributeTrackRequest(IReadOnlyList<int> DspIds);
