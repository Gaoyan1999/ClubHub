using ClubHub.Core.Models;
using ClubHub.Core.Services;

namespace ClubHub.Tests;

public class EventValidatorTests
{
    private readonly Room _room = new() { Id = 1, Name = "Lab", Capacity = 40 };

    private Workshop ValidWorkshop() => new()
    {
        Title = "Intro to Git",
        Start = new DateTime(2026, 10, 20, 17, 0, 0),
        End = new DateTime(2026, 10, 20, 19, 0, 0),
        RoomId = 1,
        Capacity = 30,
        TicketPrice = 0,
        MaterialsCostPerHead = 2
    };

    [Test]
    public void ValidEvent_HasNoErrors()
    {
        Assert.That(EventValidator.Validate(ValidWorkshop(), _room), Is.Empty);
    }

    [Test]
    public void EndBeforeStart_IsError()
    {
        var evt = ValidWorkshop();
        evt.End = evt.Start.AddHours(-1);

        Assert.That(EventValidator.Validate(evt, _room), Has.Some.Contains("End time must be after start time"));
    }

    [Test]
    public void CapacityBiggerThanRoom_IsError()
    {
        var evt = ValidWorkshop();
        evt.Capacity = 41;

        Assert.That(EventValidator.Validate(evt, _room), Has.Some.Contains("room holds (40)"));
    }

    [TestCase(0)]
    [TestCase(-5)]
    public void CapacityNotPositive_IsError(int capacity)
    {
        var evt = ValidWorkshop();
        evt.Capacity = capacity;

        Assert.That(EventValidator.Validate(evt, _room), Has.Some.Contains("Capacity must be more than 0"));
    }

    [Test]
    public void MissingTitleRoomAndNegativePrice_AreAllReported()
    {
        var evt = ValidWorkshop();
        evt.Title = " ";
        evt.TicketPrice = -1;

        var errors = EventValidator.Validate(evt, room: null);

        Assert.That(errors, Has.Count.EqualTo(3));
    }

    [Test]
    public void CompetitionRules_AreChecked()
    {
        var evt = new Competition
        {
            Title = "Hackathon",
            Start = new DateTime(2026, 10, 20, 9, 0, 0),
            End = new DateTime(2026, 10, 20, 17, 0, 0),
            Capacity = 20,
            PrizePool = -100,
            TeamSize = 0
        };

        var errors = EventValidator.Validate(evt, _room);

        Assert.That(errors, Is.EquivalentTo(new[] { "Prize money cannot be negative.", "Team size must be at least 1." }));
    }
}
