using ClubHub.Core.Enums;
using ClubHub.Core.Models;

namespace ClubHub.Core.Services;

public static class BudgetService
{
    /// <summary>
    /// Ticket income comes from people who actually checked in. The estimated cost uses the
    /// event's own <see cref="Event.EstimateCost"/> rule for the number of people going.
    /// </summary>
    public static BudgetSummary Summarize(Event evt, IEnumerable<Rsvp> rsvps, IEnumerable<BudgetEntry> entries)
    {
        var rsvpList = rsvps.ToList();
        var entryList = entries.ToList();

        var going = rsvpList.Count(r => r.Status == RsvpStatus.Going);
        var checkedIn = rsvpList.Count(r => r.Status == RsvpStatus.Going && r.CheckedIn);

        return new BudgetSummary(
            CheckedIn: checkedIn,
            TicketIncome: checkedIn * evt.TicketPrice,
            OtherIncome: entryList.Where(e => e.Type == EntryType.Income).Sum(e => e.Amount),
            TotalCost: entryList.Where(e => e.Type == EntryType.Cost).Sum(e => e.Amount),
            EstimatedCost: evt.EstimateCost(going));
    }
}
