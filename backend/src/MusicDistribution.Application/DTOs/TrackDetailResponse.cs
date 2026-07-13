namespace MusicDistribution.Application.DTOs;

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
