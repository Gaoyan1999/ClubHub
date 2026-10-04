using ClubHub.Core.Enums;

namespace ClubHub.Core.Models;

public class Social : Event
{
    public decimal CateringCostPerHead { get; set; }

    public override EventType Type => EventType.Social;

    public override decimal EstimateCost(int attendees) => CateringCostPerHead * attendees;
}
