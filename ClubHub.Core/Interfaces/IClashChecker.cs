using ClubHub.Core.Models;

namespace ClubHub.Core.Interfaces;

public interface IClashChecker
{
    /// <summary>Returns the existing events that use the same room at an overlapping time.</summary>
    List<Event> FindRoomClashes(Event newEvent, IEnumerable<Event> existingEvents);
}
