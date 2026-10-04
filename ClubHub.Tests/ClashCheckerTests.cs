using ClubHub.Core.Models;
using ClubHub.Core.Services;

namespace ClubHub.Tests;

public class ClashCheckerTests
{
    private ClashChecker _checker = null!;
    private readonly DateTime _day = new(2026, 10, 20);

    [SetUp]
    public void SetUp()
    {
        _checker = new ClashChecker();
    }

    private Workshop MakeEvent(int id, int roomId, int startHour, int endHour) => new()
    {
        Id = id,
        RoomId = roomId,
        Start = _day.AddHours(startHour),
        End = _day.AddHours(endHour)
    };

    [Test]
    public void SameRoomOverlappingTime_IsClash()
    {
        var existing = MakeEvent(1, roomId: 1, startHour: 10, endHour: 12);
        var newEvent = MakeEvent(2, roomId: 1, startHour: 11, endHour: 13);

        var clashes = _checker.FindRoomClashes(newEvent, new[] { existing });

        Assert.That(clashes, Has.Count.EqualTo(1));
    }

    [Test]
    public void SameRoomBackToBack_IsNotClash()
    {
        var existing = MakeEvent(1, roomId: 1, startHour: 10, endHour: 12);
        var newEvent = MakeEvent(2, roomId: 1, startHour: 12, endHour: 14);

        var clashes = _checker.FindRoomClashes(newEvent, new[] { existing });

        Assert.That(clashes, Is.Empty);
    }

    [Test]
    public void DifferentRoomSameTime_IsNotClash()
    {
        var existing = MakeEvent(1, roomId: 1, startHour: 10, endHour: 12);
        var newEvent = MakeEvent(2, roomId: 2, startHour: 10, endHour: 12);

        var clashes = _checker.FindRoomClashes(newEvent, new[] { existing });

        Assert.That(clashes, Is.Empty);
    }
}
