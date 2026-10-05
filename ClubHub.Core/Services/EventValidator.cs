using ClubHub.Core.Models;

namespace ClubHub.Core.Services;

public static class EventValidator
{
    /// <summary>Returns a list of problems with the event. An empty list means the event is valid.</summary>
    public static List<string> Validate(Event evt, Room? room)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(evt.Title))
            errors.Add("Title is required.");

        if (room is null)
            errors.Add("Please choose a room.");

        if (evt.End <= evt.Start)
            errors.Add("End time must be after start time.");

        if (evt.Capacity <= 0)
            errors.Add("Capacity must be more than 0.");
        else if (room is not null && evt.Capacity > room.Capacity)
            errors.Add($"Capacity cannot be more than the room holds ({room.Capacity}).");

        if (evt.TicketPrice < 0)
            errors.Add("Ticket price cannot be negative.");

        // Each event type has its own extra rules
        switch (evt)
        {
            case Workshop w when w.MaterialsCostPerHead < 0:
                errors.Add("Materials cost cannot be negative.");
                break;
            case Social s when s.CateringCostPerHead < 0:
                errors.Add("Food cost cannot be negative.");
                break;
            case Competition c:
                if (c.PrizePool < 0)
                    errors.Add("Prize money cannot be negative.");
                if (c.TeamSize < 1)
                    errors.Add("Team size must be at least 1.");
                break;
        }

        return errors;
    }
}
