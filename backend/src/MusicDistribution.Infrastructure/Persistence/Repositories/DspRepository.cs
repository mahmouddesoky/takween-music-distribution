using Microsoft.EntityFrameworkCore;
using MusicDistribution.Application.Abstractions.Persistence;
using MusicDistribution.Domain.Entities;

namespace MusicDistribution.Infrastructure.Persistence.Repositories;

internal sealed class DspRepository(AppDbContext context) : IDspRepository
{
    public async Task<IReadOnlyList<Dsp>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Dsps
            .AsNoTracking()
            .OrderBy(dsp => dsp.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<int>> GetExistingIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default) =>
        await context.Dsps
            .Where(dsp => ids.Contains(dsp.Id))
            .Select(dsp => dsp.Id)
            .ToListAsync(cancellationToken);
}
