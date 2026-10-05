using ClubHub.Core.Enums;
using ClubHub.Core.Models;

namespace ClubHub.Tests;

public class EventTests
{
    // Polymorphism: the same call gives a different cost rule for each event type
    [Test]
    public void EstimateCost_UsesEachEventTypesOwnRule()
    {
        var events = new Event[]
        {
            new Workshop { MaterialsCostPerHead = 2 },
            new Social { CateringCostPerHead = 8 },
            new Competition { PrizePool = 500 }
        };

        var costs = events.Select(e => e.EstimateCost(attendees: 10)).ToList();

        Assert.That(costs, Is.EqualTo(new[] { 20m, 80m, 500m }));
    }

    [Test]
    public void Type_MatchesSubclass()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Workshop().Type, Is.EqualTo(EventType.Workshop));
            Assert.That(new Social().Type, Is.EqualTo(EventType.Social));
            Assert.That(new Competition().Type, Is.EqualTo(EventType.Competition));
        });
    }

    [Test]
    public void Counts_OnlyIncludeMatchingStatus()
    {
        var evt = new Social { Capacity = 2 };
        evt.Rsvps.AddRange(new[]
        {
            new Rsvp { Status = RsvpStatus.Going, CheckedIn = true },
            new Rsvp { Status = RsvpStatus.Going },
            new Rsvp { Status = RsvpStatus.Waitlisted },
            new Rsvp { Status = RsvpStatus.Cancelled, CheckedIn = true }
        });

        Assert.Multiple(() =>
        {
            Assert.That(evt.GoingCount, Is.EqualTo(2));
            Assert.That(evt.WaitlistCount, Is.EqualTo(1));
            Assert.That(evt.CheckedInCount, Is.EqualTo(1));
            Assert.That(evt.IsFull, Is.True);
        });
    }
}
