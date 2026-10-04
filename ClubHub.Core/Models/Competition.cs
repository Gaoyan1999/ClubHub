using ClubHub.Core.Enums;

namespace ClubHub.Core.Models;

public class Competition : Event
{
    public decimal PrizePool { get; set; }
    public int TeamSize { get; set; } = 1;

    public override EventType Type => EventType.Competition;

    // Prize pool is a fixed cost, it does not grow with attendees
    public override decimal EstimateCost(int attendees) => PrizePool;
}
