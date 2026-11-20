
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CareFlow.Infrastructure.Persistence;

public sealed class CareFlowDbContextFactory
    : IDesignTimeDbContextFactory<CareFlowDbContext>
{
    public CareFlowDbContext CreateDbContext(string[] args)
    {
        string dataDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "CareFlow");

        Directory.CreateDirectory(dataDirectory);

        string databasePath = Path.Combine(
            dataDirectory,
            "careflow.db");

        var optionsBuilder =
            new DbContextOptionsBuilder<CareFlowDbContext>();

        optionsBuilder.UseSqlite(
            $"Data Source={databasePath};Default Timeout=30");

        return new CareFlowDbContext(optionsBuilder.Options);
    }
}
