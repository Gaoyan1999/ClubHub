using ClubHub.Core.Models;

namespace ClubHub.Core.Services;

public static class BudgetEntryValidator
{
    public static List<string> Validate(BudgetEntry entry)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(entry.Description))
            errors.Add("Description is required.");

        if (entry.Amount <= 0)
            errors.Add("Amount must be more than 0.");

        return errors;
    }
}
