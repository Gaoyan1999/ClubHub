using ClubHub.Core.Enums;
using ClubHub.Core.Models;

namespace ClubHub.Core.Services;

/// <summary>
/// Numbers for the Dashboard. Events passed in need their Rsvps and BudgetEntries loaded.
/// </summary>
public static class StatsService
{
    /// <summary>Events that start between the two dates (both days included), oldest first.</summary>
    public static List<Event> InRange(IEnumerable<Event> events, DateTime from, DateTime to) =>
        events.Where(e => e.Start.Date >= from.Date && e.Start.Date <= to.Date)
              .OrderBy(e => e.Start)
              .ToList();

    public static int CountUpcoming(IEnumerable<Event> events, DateTime now) =>
        events.Count(e => e.Start > now);

    /// <summary>
    /// Share of people going to finished events who checked in.
    /// Null when no one was going to a finished event, so there is nothing to measure.
    /// </summary>
    public static double? AttendanceRate(IEnumerable<Event> events, DateTime now)
    {
        var finished = events.Where(e => e.End < now).ToList();
        var going = finished.Sum(e => e.GoingCount);
        if (going == 0)
            return null;

        return (double)finished.Sum(e => e.CheckedInCount) / going;
    }

    /// <summary>Share of people going to finished events who did not turn up.</summary>
    public static double? NoShowRate(IEnumerable<Event> events, DateTime now) =>
        1 - AttendanceRate(events, now);

    public static Dictionary<EventType, int> CountByType(IEnumerable<Event> events) =>
        events.GroupBy(e => e.Type)
              .OrderBy(g => g.Key)
              .ToDictionary(g => g.Key, g => g.Count());

    /// <summary>
    /// Totals per month from the first to the last event's month. Months with no events
    /// are included as zeros so a line chart does not skip them.
    /// </summary>
    public static List<MonthlyTotals> TotalsByMonth(IEnumerable<Event> events)
    {
        var byMonth = events.GroupBy(e => new DateTime(e.Start.Year, e.Start.Month, 1))
                            .ToDictionary(g => g.Key, g => g.ToList());
        if (byMonth.Count == 0)
            return new List<MonthlyTotals>();

        var totals = new List<MonthlyTotals>();
        for (var month = byMonth.Keys.Min(); month <= byMonth.Keys.Max(); month = month.AddMonths(1))
        {
            var monthEvents = byMonth.GetValueOrDefault(month) ?? new List<Event>();
            var summaries = monthEvents.Select(e => BudgetService.Summarize(e, e.Rsvps, e.BudgetEntries)).ToList();
            totals.Add(new MonthlyTotals(
                month,
                CheckedIn: monthEvents.Sum(e => e.CheckedInCount),
                Income: summaries.Sum(s => s.TotalIncome),
                Cost: summaries.Sum(s => s.TotalCost)));
        }

        return totals;
    }
}
