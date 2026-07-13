namespace MusicDistribution.Application.DTOs;

public record TrackSummaryResponse(
    int Id,
    string Title,
    int ArtistId,
    string ArtistName,
    string Isrc,
    DateOnly ReleaseDate,
    string Genre,
    string Status);
