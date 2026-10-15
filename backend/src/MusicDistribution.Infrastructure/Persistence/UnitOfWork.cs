using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MusicDistribution.Application.Abstractions.Persistence;
using MusicDistribution.Application.Common.Exceptions;

namespace MusicDistribution.Infrastructure.Persistence;

internal sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    // SQLITE_CONSTRAINT_UNIQUE and SQLITE_CONSTRAINT_PRIMARYKEY. Other constraint failures
    // (foreign key, not null) are bugs, not conflicts, so they stay 500 and get logged.
    private const int SqliteUniqueViolation = 2067;
    private const int SqlitePrimaryKeyViolation = 1555;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException
            {
                SqliteExtendedErrorCode: SqliteUniqueViolation or SqlitePrimaryKeyViolation
            })
        {
            // The pre-checks in the services catch the normal case; this covers two requests
            // racing past them (e.g. same ISRC at the same time) so the client gets 409, not 500.
            throw new ConflictException("The change conflicts with existing data (a unique value is already in use).");
        }
    }
}
