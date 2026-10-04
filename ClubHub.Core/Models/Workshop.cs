using ClubHub.Core.Enums;

namespace ClubHub.Core.Models;

public class Workshop : Event
{
    public decimal MaterialsCostPerHead { get; set; }

    public override EventType Type => EventType.Workshop;

    public override decimal EstimateCost(int attendees) => MaterialsCostPerHead * attendees;
}
