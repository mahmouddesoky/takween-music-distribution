using Microsoft.EntityFrameworkCore;
using MusicDistribution.Domain.Entities;
using MusicDistribution.Domain.Enums;

namespace MusicDistribution.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds an empty database with reference DSPs, artists, tracks and distributions.
/// Idempotent: runs only when no DSPs exist yet.
/// </summary>
internal static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Dsps.AnyAsync(cancellationToken))
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
            Track.Seeded(1, "Tamally Maak", 1, "EGABC1250001", new DateOnly(2025, 3, 14), "Pop", TrackStatus.Draft),
            Track.Seeded(2, "Nour El Ain", 1, "EGABC1250002", new DateOnly(2025, 5, 2), "Pop", TrackStatus.Submitted),
            Track.Seeded(3, "Odam Merayetha", 1, "EGABC1250003", new DateOnly(2024, 11, 20), "Pop", TrackStatus.Distributed),
            Track.Seeded(4, "Ya Skandaria", 2, "EGABC1250004", new DateOnly(2024, 9, 30), "Nubian", TrackStatus.Distributed),
            Track.Seeded(5, "Madad", 2, "EGABC1250005", new DateOnly(2025, 1, 8), "Tarab", TrackStatus.Submitted),
            Track.Seeded(6, "Lessa Fe Zaman", 3, "EGABC1250006", new DateOnly(2025, 2, 18), "Rap", TrackStatus.Draft),
            Track.Seeded(7, "Ghaba", 3, "EGABC1250007", new DateOnly(2025, 4, 11), "Rap", TrackStatus.Submitted),
            Track.Seeded(8, "El Batnan", 4, "EGABC1250008", new DateOnly(2024, 12, 5), "Trap", TrackStatus.Draft),
            Track.Seeded(9, "Dakkaro", 4, "EGABC1250009", new DateOnly(2025, 6, 1), "Trap", TrackStatus.Submitted),
            Track.Seeded(10, "Esmaani", 2, "EGABC1250010", new DateOnly(2025, 7, 19), "Tarab", TrackStatus.Distributed)
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
            new TrackDistribution { Id = 8, TrackId = 10, DspId = 2, SubmittedAt = new DateTime(2025, 7, 20, 11, 20, 0, DateTimeKind.Utc), Status = DistributionStatus.Live },
            new TrackDistribution { Id = 9, TrackId = 5, DspId = 1, SubmittedAt = new DateTime(2025, 1, 9, 12, 0, 0, DateTimeKind.Utc), Status = DistributionStatus.Pending },
            new TrackDistribution { Id = 10, TrackId = 7, DspId = 2, SubmittedAt = new DateTime(2025, 4, 12, 15, 30, 0, DateTimeKind.Utc), Status = DistributionStatus.Pending }
        };

        context.Dsps.AddRange(dsps);
        context.Artists.AddRange(artists);
        context.Tracks.AddRange(tracks);
        context.TrackDistributions.AddRange(distributions);

        await context.SaveChangesAsync(cancellationToken);
    }
}
