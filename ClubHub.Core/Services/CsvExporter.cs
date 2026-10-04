using System.Text;
using ClubHub.Core.Interfaces;

namespace ClubHub.Core.Services;

/// <summary>
/// Writes any list of objects to a CSV file. Each column is a header plus a lambda
/// that picks the value, e.g. <c>("Email", m => m.Email)</c>.
/// </summary>
public class CsvExporter<T> : IExporter<T>
{
    private readonly List<(string Header, Func<T, object?> Selector)> _columns;

    public CsvExporter(params (string Header, Func<T, object?> Selector)[] columns)
    {
        if (columns.Length == 0)
            throw new ArgumentException("At least one column is required.", nameof(columns));

        _columns = columns.ToList();
    }

    public void Export(IEnumerable<T> items, string filePath)
    {
        File.WriteAllText(filePath, ToCsv(items), Encoding.UTF8);
    }

    public string ToCsv(IEnumerable<T> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", _columns.Select(c => Escape(c.Header))));

        foreach (var item in items)
            sb.AppendLine(string.Join(",", _columns.Select(c => Escape(c.Selector(item)?.ToString()))));

        return sb.ToString();
    }

    // Quote a value when it contains a comma, quote or line break (standard CSV rule)
    private static string Escape(string? value)
    {
        value ??= string.Empty;
        if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
