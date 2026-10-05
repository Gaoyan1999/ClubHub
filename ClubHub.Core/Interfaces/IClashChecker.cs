using ClubHub.Core.Models;

namespace ClubHub.Core.Interfaces;

public interface IClashChecker
{
    /// <summary>Returns the existing events that use the same room at an overlapping time.</summary>
    List<Event> FindRoomClashes(Event newEvent, IEnumerable<Event> existingEvents);

    /// <summary>Returns the other events that overlap in time with the target, in any room.</summary>
    List<Event> FindTimeClashes(Event target, IEnumerable<Event> otherEvents);
}
