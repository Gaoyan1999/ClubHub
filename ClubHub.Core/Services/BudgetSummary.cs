namespace ClubHub.Core.Services;

/// <summary>Money totals for one event.</summary>
public record BudgetSummary(
    int CheckedIn,
    decimal TicketIncome,
    decimal OtherIncome,
    decimal TotalCost,
    decimal EstimatedCost)
{
    public decimal TotalIncome => TicketIncome + OtherIncome;
    public decimal Profit => TotalIncome - TotalCost;
}
