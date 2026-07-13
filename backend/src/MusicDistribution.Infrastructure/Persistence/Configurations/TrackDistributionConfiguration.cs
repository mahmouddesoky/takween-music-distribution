using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicDistribution.Domain.Entities;

namespace MusicDistribution.Infrastructure.Persistence.Configurations;

public class TrackDistributionConfiguration : IEntityTypeConfiguration<TrackDistribution>
{
    public void Configure(EntityTypeBuilder<TrackDistribution> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.SubmittedAt).IsRequired();

        // A track can only be submitted to a given DSP once.
        builder.HasIndex(d => new { d.TrackId, d.DspId }).IsUnique();

        builder.HasOne(d => d.Dsp)
            .WithMany(dsp => dsp.Distributions)
            .HasForeignKey(d => d.DspId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
