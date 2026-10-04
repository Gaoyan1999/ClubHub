using ClubHub.Core.Enums;
using ClubHub.Core.Models;
using ClubHub.Data;

namespace ClubHub.Tests;

public class RepositoryTests
{
    private ClubHubDbContext _context = null!;
    private Repository<Member> _members = null!;

    [SetUp]
    public void SetUp()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"clubhub-test-{Guid.NewGuid()}.db");
        _context = new ClubHubDbContext($"Data Source={dbPath}");
        _context.Database.EnsureCreated();
        DbSeeder.Seed(_context);
        _members = new Repository<Member>(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public void Find_ReturnsOnlyMatchingRows()
    {
        var presidents = _members.Find(m => m.Role == MemberRole.President);

        Assert.That(presidents, Has.Count.EqualTo(1));
        Assert.That(presidents[0].FirstName, Is.EqualTo("Alice"));
    }

    [Test]
    public void Add_DuplicateStudentId_ThrowsAndLaterSavesStillWork()
    {
        var clubId = _context.Clubs.First().Id;
        var duplicate = new Member("24000001", "Copy", "Cat", "copy@uts.edu.au") { ClubId = clubId };

        Assert.That(() => _members.Add(duplicate), Throws.Exception);

        // The failed member must not block the next save
        var valid = new Member("24000010", "Grace", "Hopper", "grace@uts.edu.au") { ClubId = clubId };
        _members.Add(valid);

        Assert.That(_members.GetAll(), Has.Count.EqualTo(6));
    }

    [Test]
    public void Update_And_Delete_ArePersisted()
    {
        var ben = _members.Find(m => m.StudentId == "24000002").Single();
        ben.Email = "ben.new@uts.edu.au";
        _members.Update(ben);

        var david = _members.Find(m => m.StudentId == "24000004").Single();
        _members.Delete(david);

        Assert.That(_context.Members.Single(m => m.StudentId == "24000002").Email, Is.EqualTo("ben.new@uts.edu.au"));
        Assert.That(_context.Members.Count(), Is.EqualTo(4));
    }
}
