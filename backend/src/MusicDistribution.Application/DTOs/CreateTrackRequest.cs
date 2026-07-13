namespace MusicDistribution.Application.DTOs;

public record CreateTrackRequest(
    string Title,
    int ArtistId,
    string Isrc,
    DateOnly ReleaseDate,
    string Genre,
    string? Status = null);
