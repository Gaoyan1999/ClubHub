namespace ClubHub.Core.Extensions;

public static class StringExtensions
{
    /// <summary>Case-insensitive "contains" that treats null as an empty string.</summary>
    public static bool ContainsIgnoreCase(this string? text, string value)
        => (text ?? string.Empty).Contains(value, StringComparison.OrdinalIgnoreCase);
}
