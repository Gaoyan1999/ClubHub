using ClubHub.Core.Enums;

namespace ClubHub.Core.Models;

/// <summary>
/// Base class for all club events. Subclasses override <see cref="EstimateCost"/>
/// with their own cost rules (polymorphism example).
/// </summary>
public abstract class Event
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int RoomId { get; set; }
    public Room? Room { get; set; }
    public int Capacity { get; set; }
    public decimal TicketPrice { get; set; }

    public List<Rsvp> Rsvps { get; set; } = new();
    public List<BudgetEntry> BudgetEntries { get; set; } = new();

    public abstract EventType Type { get; }

    /// <summary>Estimated total cost for the given number of attendees.</summary>
    public abstract decimal EstimateCost(int attendees);
}
