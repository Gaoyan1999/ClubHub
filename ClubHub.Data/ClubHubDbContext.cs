using ClubHub.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubHub.Data;

public class ClubHubDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Rsvp> Rsvps => Set<Rsvp>();
    public DbSet<BudgetEntry> BudgetEntries => Set<BudgetEntry>();

    public ClubHubDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>Database file in the user's local app data folder, e.g. %LOCALAPPDATA%\ClubHub\clubhub.db.</summary>
    public static string DefaultConnectionString()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ClubHub");
        Directory.CreateDirectory(folder);
        return $"Data Source={Path.Combine(folder, "clubhub.db")}";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite cannot Sum/Order decimals in SQL, so store them as REAL
        configurationBuilder.Properties<decimal>().HaveConversion<double>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // All event subclasses share one table (TPH), told apart by a discriminator column
        modelBuilder.Entity<Event>()
            .HasDiscriminator<string>("EventKind")
            .HasValue<Workshop>(nameof(Workshop))
            .HasValue<Social>(nameof(Social))
            .HasValue<Competition>(nameof(Competition));

        modelBuilder.Entity<Event>().Ignore(e => e.Type);
        modelBuilder.Entity<Member>().Ignore(m => m.FullName);

        modelBuilder.Entity<Member>().HasIndex(m => m.StudentId).IsUnique();
    }
}
