using ClubHub.Data;
using Microsoft.EntityFrameworkCore;

namespace ClubHub.Tests;

/// <summary>Tests use a throwaway SQLite file instead of the cloud PostgreSQL database.</summary>
internal static class TestDb
{
    public static ClubHubDbContext Create()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"clubhub-test-{Guid.NewGuid()}.db");
        var options = new DbContextOptionsBuilder<ClubHubDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
        return new ClubHubDbContext(options);
    }
}
