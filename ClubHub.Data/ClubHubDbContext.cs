using ClubHub.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubHub.Data;

public class ClubHubDbContext : DbContext
{
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Rsvp> Rsvps => Set<Rsvp>();
    public DbSet<BudgetEntry> BudgetEntries => Set<BudgetEntry>();

    public ClubHubDbContext(DbContextOptions<ClubHubDbContext> options) : base(options)
    {
    }

    /// <summary>Context for the app's cloud PostgreSQL database.</summary>
    public static ClubHubDbContext ForPostgres(string connectionString)
    {
        var options = new DbContextOptionsBuilder<ClubHubDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new ClubHubDbContext(options);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        if (Database.IsNpgsql())
        {
            // The app uses local times (DateTime.Now); Npgsql only accepts UTC for "timestamp with time zone"
            configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
        }
        else
        {
            // Tests run on SQLite, which cannot Sum/Order decimals in SQL, so store them as REAL there
            configurationBuilder.Properties<decimal>().HaveConversion<double>();
        }
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
