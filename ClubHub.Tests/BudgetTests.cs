using ClubHub.Core.Enums;
using ClubHub.Core.Models;
using ClubHub.Core.Services;

namespace ClubHub.Tests;

public class BudgetServiceTests
{
    [Test]
    public void Summarize_CalculatesIncomeCostAndProfit()
    {
        var evt = new Social { TicketPrice = 5, CateringCostPerHead = 8 };
        var rsvps = new[]
        {
            new Rsvp { Status = RsvpStatus.Going, CheckedIn = true },
            new Rsvp { Status = RsvpStatus.Going, CheckedIn = true },
            new Rsvp { Status = RsvpStatus.Going },
            new Rsvp { Status = RsvpStatus.Waitlisted },
            new Rsvp { Status = RsvpStatus.Cancelled, CheckedIn = true }
        };
        var entries = new[]
        {
            new BudgetEntry { Type = EntryType.Cost, Amount = 24 },
            new BudgetEntry { Type = EntryType.Cost, Amount = 6 },
            new BudgetEntry { Type = EntryType.Income, Amount = 10 }
        };

        var summary = BudgetService.Summarize(evt, rsvps, entries);

        Assert.Multiple(() =>
        {
            Assert.That(summary.CheckedIn, Is.EqualTo(2));
            Assert.That(summary.TicketIncome, Is.EqualTo(10m));     // 2 checked in x $5
            Assert.That(summary.OtherIncome, Is.EqualTo(10m));
            Assert.That(summary.TotalIncome, Is.EqualTo(20m));
            Assert.That(summary.TotalCost, Is.EqualTo(30m));
            Assert.That(summary.EstimatedCost, Is.EqualTo(24m));    // 3 going x $8
            Assert.That(summary.Profit, Is.EqualTo(-10m));
        });
    }

    [Test]
    public void Summarize_EmptyEvent_IsAllZero()
    {
        var summary = BudgetService.Summarize(new Workshop(), Array.Empty<Rsvp>(), Array.Empty<BudgetEntry>());

        Assert.That(summary.Profit, Is.Zero);
        Assert.That(summary.EstimatedCost, Is.Zero);
    }

    [Test]
    public void SummarizeEach_MatchesRsvpsAndEntriesToTheirEvent_AndAddsUpTheClubBalance()
    {
        var social = new Social { Id = 1, TicketPrice = 5 };
        var workshop = new Workshop { Id = 2, TicketPrice = 10 };
        var rsvps = new[]
        {
            new Rsvp { EventId = 1, Status = RsvpStatus.Going, CheckedIn = true },
            new Rsvp { EventId = 1, Status = RsvpStatus.Going, CheckedIn = true },
            new Rsvp { EventId = 2, Status = RsvpStatus.Going, CheckedIn = true }
        };
        var entries = new[]
        {
            new BudgetEntry { EventId = 1, Type = EntryType.Cost, Amount = 4 },
            new BudgetEntry { EventId = 2, Type = EntryType.Cost, Amount = 30 },
            new BudgetEntry { EventId = 2, Type = EntryType.Income, Amount = 5 }
        };

        var summaries = BudgetService.SummarizeEach(new Event[] { social, workshop }, rsvps, entries);

        Assert.Multiple(() =>
        {
            Assert.That(summaries[0].Event, Is.SameAs(social));
            Assert.That(summaries[0].Summary.TotalIncome, Is.EqualTo(10m));   // 2 x $5 tickets
            Assert.That(summaries[0].Summary.TotalCost, Is.EqualTo(4m));
            Assert.That(summaries[1].Summary.TotalIncome, Is.EqualTo(15m));   // $10 ticket + $5 income
            Assert.That(summaries[1].Summary.TotalCost, Is.EqualTo(30m));
            Assert.That(BudgetService.ClubBalance(summaries.Select(s => s.Summary)), Is.EqualTo(-9m));  // 6 profit - 15 loss
        });
    }
}

public class BudgetEntryValidatorTests
{
    [Test]
    public void ValidEntry_HasNoErrors()
    {
        var entry = new BudgetEntry { Description = "Pizza", Amount = 240, Type = EntryType.Cost };

        Assert.That(BudgetEntryValidator.Validate(entry), Is.Empty);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void AmountNotPositive_IsError(decimal amount)
    {
        var entry = new BudgetEntry { Description = "Pizza", Amount = amount };

        Assert.That(BudgetEntryValidator.Validate(entry), Has.Some.Contains("Amount must be more than 0"));
    }

    [Test]
    public void BlankDescription_IsError()
    {
        var entry = new BudgetEntry { Description = "  ", Amount = 5 };

        Assert.That(BudgetEntryValidator.Validate(entry), Has.Some.Contains("Description is required"));
    }
}
