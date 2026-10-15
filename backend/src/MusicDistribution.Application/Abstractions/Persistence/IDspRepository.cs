using MusicDistribution.Domain.Entities;

namespace MusicDistribution.Application.Abstractions.Persistence;

public interface IDspRepository
{
    Task<IReadOnlyList<Dsp>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetExistingIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);
}
