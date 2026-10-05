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

        var alice = new Member("24000001", "Alice", "Nguyen", "alice.nguyen@student.uts.edu.au", MemberRole.President);
        var ben = new Member("24000002", "Ben", "Smith", "ben.smith@student.uts.edu.au", MemberRole.Treasurer);
        var chloe = new Member("24000003", "Chloe", "Wang", "chloe.wang@student.uts.edu.au", MemberRole.Secretary);
        var david = new Member("24000004", "David", "Lee", "david.lee@student.uts.edu.au");
        var emma = new Member("24000005", "Emma", "Patel", "emma.patel@student.uts.edu.au");
        club.Members.AddRange(new[] { alice, ben, chloe, david, emma });

        var lab = new Room { Name = "CB11.04.101", Capacity = 40 };
        var hall = new Room { Name = "CB06 Great Hall", Capacity = 200 };
        var alumni = new Room { Name = "Alumni Green", Capacity = 150, IsOutdoor = true };
        var meetingRoom = new Room { Name = "CB02.05.012", Capacity = 10 };
        context.Rooms.AddRange(lab, hall, alumni, meetingRoom);

        var nextWeek = DateTime.Today.AddDays(7);
        var created = DateTime.Now.AddDays(-3);

        var introToGit = new Workshop
        {
            Title = "Intro to Git", Start = nextWeek.AddHours(17), End = nextWeek.AddHours(19),
            Room = lab, Capacity = 30, TicketPrice = 0, MaterialsCostPerHead = 2
        };
        AddRsvps(introToGit, created, alice, ben, chloe);

        var pizzaNight = new Social
        {
            Title = "Pizza Night", Start = nextWeek.AddDays(2).AddHours(18), End = nextWeek.AddDays(2).AddHours(21),
            Room = alumni, Capacity = 100, TicketPrice = 5, CateringCostPerHead = 8
        };
        AddRsvps(pizzaNight, created, alice, ben, chloe, david, emma);
        pizzaNight.BudgetEntries.AddRange(new[]
        {
            new BudgetEntry { Type = EntryType.Cost, Amount = 240, Description = "Pizza order", Date = nextWeek.AddDays(1) },
            new BudgetEntry { Type = EntryType.Cost, Amount = 60, Description = "Drinks", Date = nextWeek.AddDays(1) },
            new BudgetEntry { Type = EntryType.Income, Amount = 100, Description = "Sponsor: local cafe", Date = DateTime.Today }
        });

        var hackathon = new Competition
        {
            Title = "Mini Hackathon", Start = nextWeek.AddDays(5).AddHours(9), End = nextWeek.AddDays(5).AddHours(17),
            Room = hall, Capacity = 120, TicketPrice = 10, PrizePool = 500, TeamSize = 3
        };
        AddRsvps(hackathon, created, ben, david);
        hackathon.BudgetEntries.Add(
            new BudgetEntry { Type = EntryType.Cost, Amount = 500, Description = "Prize money", Date = nextWeek.AddDays(5) });

        // Small room so the waitlist can be shown straight away: 2 going, 1 waiting
        var resumeClinic = new Workshop
        {
            Title = "Resume Clinic", Start = nextWeek.AddDays(3).AddHours(12), End = nextWeek.AddDays(3).AddHours(13),
            Room = meetingRoom, Capacity = 2, TicketPrice = 0, MaterialsCostPerHead = 1
        };
        AddRsvps(resumeClinic, created, chloe, emma, david);

        club.Events.AddRange(new Event[] { introToGit, pizzaNight, hackathon, resumeClinic });

        context.Clubs.Add(club);
        context.SaveChanges();
    }

    // Registers members in order; once the event is full the rest go on the waitlist
    private static void AddRsvps(Event evt, DateTime firstCreated, params Member[] members)
    {
        for (var i = 0; i < members.Length; i++)
        {
            evt.Rsvps.Add(new Rsvp
            {
                Member = members[i],
                Status = i < evt.Capacity ? RsvpStatus.Going : RsvpStatus.Waitlisted,
                CreatedAt = firstCreated.AddMinutes(i)
            });
        }
    }
}
