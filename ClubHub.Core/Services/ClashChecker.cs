using ClubHub.Core.Extensions;
using ClubHub.Core.Interfaces;
using ClubHub.Core.Models;

namespace ClubHub.Core.Services;

public class ClashChecker : IClashChecker
{
    public List<Event> FindRoomClashes(Event newEvent, IEnumerable<Event> existingEvents)
    {
        return existingEvents
            .Where(e => e.Id != newEvent.Id && e.RoomId == newEvent.RoomId)
            .Where(e => newEvent.Start.Overlaps(newEvent.End, e.Start, e.End))
            .ToList();
    }

    public List<Event> FindTimeClashes(Event target, IEnumerable<Event> otherEvents)
    {
        return otherEvents
            .Where(e => e.Id != target.Id && target.Start.Overlaps(target.End, e.Start, e.End))
            .ToList();
    }
}
