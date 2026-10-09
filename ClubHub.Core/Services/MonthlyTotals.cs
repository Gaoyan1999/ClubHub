namespace ClubHub.Core.Services;

/// <summary>Check-ins and money for all events that start in one month.</summary>
public record MonthlyTotals(DateTime Month, int CheckedIn, decimal Income, decimal Cost);
