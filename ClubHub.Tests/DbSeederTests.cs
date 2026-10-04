using ClubHub.Core.Models;
using ClubHub.Data;

namespace ClubHub.Tests;

public class DbSeederTests
{
    private string _dbPath = null!;
    private ClubHubDbContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        // Each test gets its own throwaway database file
        _dbPath = Path.Combine(Path.GetTempPath(), $"clubhub-test-{Guid.NewGuid()}.db");
        _context = new ClubHubDbContext($"Data Source={_dbPath}");
        _context.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public void Seed_EmptyDatabase_AddsStarterData()
    {
        DbSeeder.Seed(_context);

        Assert.That(_context.Clubs.Count(), Is.EqualTo(1));
        Assert.That(_context.Members.Count(), Is.EqualTo(5));
        Assert.That(_context.Events.OfType<Competition>().Count(), Is.EqualTo(1));
    }

    [Test]
    public void Seed_RunTwice_DoesNotDuplicate()
    {
        DbSeeder.Seed(_context);
        DbSeeder.Seed(_context);

        Assert.That(_context.Clubs.Count(), Is.EqualTo(1));
    }
}
