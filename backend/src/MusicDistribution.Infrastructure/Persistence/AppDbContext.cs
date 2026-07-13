using Microsoft.EntityFrameworkCore;
using MusicDistribution.Domain.Entities;

namespace MusicDistribution.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Dsp> Dsps => Set<Dsp>();
    public DbSet<TrackDistribution> TrackDistributions => Set<TrackDistribution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
