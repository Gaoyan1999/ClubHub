using ClubHub.Core.Models;
using ClubHub.Core.Services;

namespace ClubHub.Tests;

public class CsvExporterTests
{
    private readonly CsvExporter<Member> _exporter = new(
        ("Student ID", m => m.StudentId),
        ("Name", m => m.FullName),
        ("Role", m => m.Role));

    [Test]
    public void WritesHeaderThenOneLinePerItem()
    {
        var members = new[]
        {
            new Member("24000001", "Alice", "Nguyen", "a@uts.edu.au"),
            new Member("24000002", "Ben", "Smith", "b@uts.edu.au")
        };

        var lines = _exporter.ToCsv(members).TrimEnd().Split(Environment.NewLine);

        Assert.That(lines, Is.EqualTo(new[]
        {
            "Student ID,Name,Role",
            "24000001,Alice Nguyen,Member",
            "24000002,Ben Smith,Member"
        }));
    }

    [Test]
    public void ValueWithCommaOrQuote_IsQuoted()
    {
        var member = new Member("24000003", "Chloe \"CJ\"", "Wang, Jr", "c@uts.edu.au");

        var dataLine = _exporter.ToCsv(new[] { member }).TrimEnd().Split(Environment.NewLine)[1];

        Assert.That(dataLine, Is.EqualTo("24000003,\"Chloe \"\"CJ\"\" Wang, Jr\",Member"));
    }

    [Test]
    public void Export_WritesFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"members-{Guid.NewGuid()}.csv");
        try
        {
            _exporter.Export(new[] { new Member("24000001", "Alice", "Nguyen", "a@uts.edu.au") }, path);

            Assert.That(File.ReadAllText(path), Does.StartWith("Student ID,Name,Role"));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
