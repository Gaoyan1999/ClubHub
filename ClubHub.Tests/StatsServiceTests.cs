using ClubHub.Core.Enums;
using ClubHub.Core.Models;
using ClubHub.Core.Services;

namespace ClubHub.Tests;

public class StatsServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 15, 12, 0, 0);

    // An event on the given day with the given RSVPs
    private static Event MakeEvent(Event evt, DateTime start, params Rsvp[] rsvps)
    {
        evt.Start = start;
        evt.End = start.AddHours(2);
        evt.Rsvps.AddRange(rsvps);
        return evt;
    }

    private static Rsvp Going(bool checkedIn) => new() { Status = RsvpStatus.Going, CheckedIn = checkedIn };

    [Test]
    public void AttendanceRate_UsesOnlyFinishedEvents()
    {
        var events = new[]
        {
            // Finished: 3 going, 2 checked in
            MakeEvent(new Workshop(), Now.AddDays(-10), Going(true), Going(true), Going(false)),
            // Finished: 1 going, 1 checked in; waitlisted and cancelled people are not counted
            MakeEvent(new Social(), Now.AddDays(-5), Going(true),
                new Rsvp { Status = RsvpStatus.Waitlisted }, new Rsvp { Status = RsvpStatus.Cancelled, CheckedIn = true }),
            // Upcoming: no one has checked in yet, so it must not pull the rate down
            MakeEvent(new Competition(), Now.AddDays(5), Going(false), Going(false))
        };

        Assert.Multiple(() =>
        {
            Assert.That(StatsService.AttendanceRate(events, Now), Is.EqualTo(0.75).Within(1e-9));   // 3 of 4
            Assert.That(StatsService.NoShowRate(events, Now), Is.EqualTo(0.25).Within(1e-9));
        });
    }

    [Test]
    public void AttendanceRate_IsNull_WhenNoFinishedEventHasPeopleGoing()
    {
        var events = new[]
        {
            MakeEvent(new Workshop(), Now.AddDays(-3)),
            MakeEvent(new Social(), Now.AddDays(3), Going(false))
        };

        Assert.Multiple(() =>
        {
            Assert.That(StatsService.AttendanceRate(events, Now), Is.Null);
            Assert.That(StatsService.NoShowRate(events, Now), Is.Null);
        });
    }

    [Test]
    public void InRange_IncludesBothEndDays_AndCountUpcoming()
    {
        var events = new[]
        {
            MakeEvent(new Workshop(), new DateTime(2026, 9, 30, 18, 0, 0)),
            MakeEvent(new Workshop(), new DateTime(2026, 10, 1, 18, 0, 0)),
            MakeEvent(new Social(), new DateTime(2026, 10, 20, 18, 0, 0)),
            MakeEvent(new Social(), new DateTime(2026, 10, 31, 18, 0, 0)),
            MakeEvent(new Social(), new DateTime(2026, 11, 1, 18, 0, 0))
        };

        var inRange = StatsService.InRange(events, new DateTime(2026, 10, 1), new DateTime(2026, 10, 31));

        Assert.Multiple(() =>
        {
            Assert.That(inRange, Has.Count.EqualTo(3));
            Assert.That(StatsService.CountUpcoming(inRange, Now), Is.EqualTo(2));
        });
    }

    [Test]
    public void CountByType_GroupsEventsByType()
    {
        var events = new Event[] { new Workshop(), new Workshop(), new Social() };

        var counts = StatsService.CountByType(events);

        Assert.Multiple(() =>
        {
            Assert.That(counts[EventType.Workshop], Is.EqualTo(2));
            Assert.That(counts[EventType.Social], Is.EqualTo(1));
            Assert.That(counts.ContainsKey(EventType.Competition), Is.False);
        });
    }

    [Test]
    public void TotalsByMonth_AddsMoneyPerMonth_AndFillsEmptyMonths()
    {
        var august = MakeEvent(new Social { TicketPrice = 5 }, new DateTime(2026, 8, 10), Going(true), Going(true));
        august.BudgetEntries.Add(new BudgetEntry { Type = EntryType.Cost, Amount = 30 });
        var october = MakeEvent(new Workshop(), new DateTime(2026, 10, 5));
        october.BudgetEntries.Add(new BudgetEntry { Type = EntryType.Income, Amount = 50 });

        var totals = StatsService.TotalsByMonth(new[] { october, august });

        Assert.Multiple(() =>
        {
            Assert.That(totals.Select(t => t.Month.Month), Is.EqualTo(new[] { 8, 9, 10 }));
            Assert.That(totals[0], Is.EqualTo(new MonthlyTotals(new DateTime(2026, 8, 1), 2, 10m, 30m)));  // 2 x $5 tickets
            Assert.That(totals[1], Is.EqualTo(new MonthlyTotals(new DateTime(2026, 9, 1), 0, 0m, 0m)));
            Assert.That(totals[2], Is.EqualTo(new MonthlyTotals(new DateTime(2026, 10, 1), 0, 50m, 0m)));
        });
    }

    [Test]
    public void TotalsByMonth_IsEmpty_WhenThereAreNoEvents()
    {
        Assert.That(StatsService.TotalsByMonth(Array.Empty<Event>()), Is.Empty);
    }
}
