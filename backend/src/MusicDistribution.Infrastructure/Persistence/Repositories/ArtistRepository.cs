using Microsoft.EntityFrameworkCore;
using MusicDistribution.Application.Abstractions.Persistence;
using MusicDistribution.Domain.Entities;

namespace MusicDistribution.Infrastructure.Persistence.Repositories;

internal sealed class ArtistRepository(AppDbContext context) : IArtistRepository
{
    public async Task<IReadOnlyList<Artist>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Artists
            .AsNoTracking()
            .OrderBy(artist => artist.Name)
            .ToListAsync(cancellationToken);

    public Task<Artist?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Artists
            .AsNoTracking()
            .FirstOrDefaultAsync(artist => artist.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        context.Artists.AnyAsync(artist => artist.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        context.Artists.AnyAsync(artist => artist.Email == email, cancellationToken);

    public void Add(Artist artist) => context.Artists.Add(artist);
}
