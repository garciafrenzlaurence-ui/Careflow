using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Persistence;

public sealed class DatabaseInitializer(IDbContextFactory<CareFlowDbContext> contextFactory)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await db.Database.MigrateAsync(cancellationToken);
    }
}
