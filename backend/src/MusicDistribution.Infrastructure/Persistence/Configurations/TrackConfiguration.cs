using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicDistribution.Domain.Entities;

namespace MusicDistribution.Infrastructure.Persistence.Configurations;

public class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).IsRequired().HasMaxLength(300);
        builder.Property(t => t.Isrc).IsRequired().HasMaxLength(12);
        builder.Property(t => t.Genre).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

        // ISRC is globally unique per recording.
        builder.HasIndex(t => t.Isrc).IsUnique();

        builder.HasMany(t => t.Distributions)
            .WithOne(d => d.Track)
            .HasForeignKey(d => d.TrackId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
