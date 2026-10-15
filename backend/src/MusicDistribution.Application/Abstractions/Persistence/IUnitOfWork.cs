namespace MusicDistribution.Application.Abstractions.Persistence;

/// <summary>
/// Commits every change tracked by the repositories in one transaction.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
