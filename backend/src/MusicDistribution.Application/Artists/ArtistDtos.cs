namespace MusicDistribution.Application.Artists;

public record ArtistResponse(int Id, string Name, string Email, string Country);

public record CreateArtistRequest(string Name, string Email, string Country);
