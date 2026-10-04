using ClubHub.Core.Enums;
using ClubHub.Core.Models;

namespace ClubHub.Data;

/// <summary>Fills an empty database with starter data so every screen has something to show.</summary>
public static class DbSeeder
{
    public static void Seed(ClubHubDbContext context)
    {
        if (context.Clubs.Any())
            return;

        var club = new Club
        {
            Name = "UTS Programmers' Society",
            Description = "Workshops, hackathons and socials for students who like to code."
        };

        club.Members.AddRange(new[]
        {
            new Member("24000001", "Alice", "Nguyen", "alice.nguyen@student.uts.edu.au", MemberRole.President),
            new Member("24000002", "Ben", "Smith", "ben.smith@student.uts.edu.au", MemberRole.Treasurer),
            new Member("24000003", "Chloe", "Wang", "chloe.wang@student.uts.edu.au", MemberRole.Secretary),
            new Member("24000004", "David", "Lee", "david.lee@student.uts.edu.au"),
            new Member("24000005", "Emma", "Patel", "emma.patel@student.uts.edu.au")
        });

        var lab = new Room { Name = "CB11.04.101", Capacity = 40 };
        var hall = new Room { Name = "CB06 Great Hall", Capacity = 200 };
        var alumni = new Room { Name = "Alumni Green", Capacity = 150, IsOutdoor = true };
        context.Rooms.AddRange(lab, hall, alumni);

        var nextWeek = DateTime.Today.AddDays(7);
        club.Events.AddRange(new Event[]
        {
            new Workshop
            {
                Title = "Intro to Git", Start = nextWeek.AddHours(17), End = nextWeek.AddHours(19),
                Room = lab, Capacity = 30, TicketPrice = 0, MaterialsCostPerHead = 2
            },
            new Social
            {
                Title = "Pizza Night", Start = nextWeek.AddDays(2).AddHours(18), End = nextWeek.AddDays(2).AddHours(21),
                Room = alumni, Capacity = 100, TicketPrice = 5, CateringCostPerHead = 8
            },
            new Competition
            {
                Title = "Mini Hackathon", Start = nextWeek.AddDays(5).AddHours(9), End = nextWeek.AddDays(5).AddHours(17),
                Room = hall, Capacity = 120, TicketPrice = 10, PrizePool = 500, TeamSize = 3
            }
        });

        context.Clubs.Add(club);
        context.SaveChanges();
    }
}
