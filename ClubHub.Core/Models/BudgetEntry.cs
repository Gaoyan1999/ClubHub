using ClubHub.Core.Enums;

namespace ClubHub.Core.Models;

public class BudgetEntry
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public EntryType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
}
