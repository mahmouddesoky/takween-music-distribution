using Microsoft.EntityFrameworkCore;
using MusicDistribution.Domain.Entities;
using MusicDistribution.Domain.Enums;
using MusicDistribution.Infrastructure.Persistence;

namespace MusicDistribution.Infrastructure.Data;

/// <summary>
/// Seeds an empty database with reference DSPs, artists, tracks and distributions.
/// Idempotent: runs only when no DSPs exist yet.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Dsps.AnyAsync())
        {
            return;
        }

        var dsps = new[]
        {
            new Dsp { Id = 1, Name = "Spotify" },
            new Dsp { Id = 2, Name = "Apple Music" },
            new Dsp { Id = 3, Name = "YouTube" }
        };

        var artists = new[]
        {
            new Artist { Id = 1, Name = "Amr Diab", Email = "booking@amrdiab.com", Country = "Egypt" },
            new Artist { Id = 2, Name = "Mohamed Mounir", Email = "management@mounir.com", Country = "Egypt" },
            new Artist { Id = 3, Name = "Marwan Moussa", Email = "marwan.moussa@example.com", Country = "Egypt" },
            new Artist { Id = 4, Name = "Wegz", Email = "booking@wegz.com", Country = "Egypt" }
        };

        var tracks = new[]
        {
            new Track { Id = 1, Title = "Tamally Maak", ArtistId = 1, Isrc = "EGABC1250001", ReleaseDate = new DateOnly(2025, 3, 14), Genre = "Pop", Status = TrackStatus.Draft },
            new Track { Id = 2, Title = "Nour El Ain", ArtistId = 1, Isrc = "EGABC1250002", ReleaseDate = new DateOnly(2025, 5, 2), Genre = "Pop", Status = TrackStatus.Submitted },
            new Track { Id = 3, Title = "Odam Merayetha", ArtistId = 1, Isrc = "EGABC1250003", ReleaseDate = new DateOnly(2024, 11, 20), Genre = "Pop", Status = TrackStatus.Distributed },
            new Track { Id = 4, Title = "Ya Skandaria", ArtistId = 2, Isrc = "EGABC1250004", ReleaseDate = new DateOnly(2024, 9, 30), Genre = "Nubian", Status = TrackStatus.Distributed },
            new Track { Id = 5, Title = "Madad", ArtistId = 2, Isrc = "EGABC1250005", ReleaseDate = new DateOnly(2025, 1, 8), Genre = "Tarab", Status = TrackStatus.Submitted },
            new Track { Id = 6, Title = "Lessa Fe Zaman", ArtistId = 3, Isrc = "EGABC1250006", ReleaseDate = new DateOnly(2025, 2, 18), Genre = "Rap", Status = TrackStatus.Draft },
            new Track { Id = 7, Title = "Ghaba", ArtistId = 3, Isrc = "EGABC1250007", ReleaseDate = new DateOnly(2025, 4, 11), Genre = "Rap", Status = TrackStatus.Submitted },
            new Track { Id = 8, Title = "El Batnan", ArtistId = 4, Isrc = "EGABC1250008", ReleaseDate = new DateOnly(2024, 12, 5), Genre = "Trap", Status = TrackStatus.Draft },
            new Track { Id = 9, Title = "Dakkaro", ArtistId = 4, Isrc = "EGABC1250009", ReleaseDate = new DateOnly(2025, 6, 1), Genre = "Trap", Status = TrackStatus.Submitted },
            new Track { Id = 10, Title = "Esmaani", ArtistId = 2, Isrc = "EGABC1250010", ReleaseDate = new DateOnly(2025, 7, 19), Genre = "Tarab", Status = TrackStatus.Distributed }
        };

        var distributions = new[]
        {
            new TrackDistribution { Id = 1, TrackId = 2, DspId = 1, SubmittedAt = new DateTime(2025, 5, 3, 10, 0, 0, DateTimeKind.Utc), Status = DistributionStatus.Pending },
            new TrackDistribution { Id = 2, TrackId = 2, DspId = 2, SubmittedAt = new DateTime(2025, 5, 3, 10, 0, 0, DateTimeKind.Utc), Status = DistributionStatus.Pending },
            new TrackDistribution { Id = 3, TrackId = 3, DspId = 1, SubmittedAt = new DateTime(2024, 11, 25, 14, 30, 0, DateTimeKind.Utc), Status = DistributionStatus.Live },
            new TrackDistribution { Id = 4, TrackId = 3, DspId = 2, SubmittedAt = new DateTime(2024, 11, 25, 14, 30, 0, DateTimeKind.Utc), Status = DistributionStatus.Live },
            new TrackDistribution { Id = 5, TrackId = 3, DspId = 3, SubmittedAt = new DateTime(2024, 11, 26, 9, 15, 0, DateTimeKind.Utc), Status = DistributionStatus.Rejected },
            new TrackDistribution { Id = 6, TrackId = 4, DspId = 1, SubmittedAt = new DateTime(2024, 10, 1, 8, 0, 0, DateTimeKind.Utc), Status = DistributionStatus.Live },
            new TrackDistribution { Id = 7, TrackId = 9, DspId = 3, SubmittedAt = new DateTime(2024, 12, 6, 16, 45, 0, DateTimeKind.Utc), Status = DistributionStatus.Pending },
            new TrackDistribution { Id = 8, TrackId = 10, DspId = 2, SubmittedAt = new DateTime(2025, 7, 20, 11, 20, 0, DateTimeKind.Utc), Status = DistributionStatus.Live }
        };

        context.Dsps.AddRange(dsps);
        context.Artists.AddRange(artists);
        context.Tracks.AddRange(tracks);
        context.TrackDistributions.AddRange(distributions);

        await context.SaveChangesAsync();
    }
}
