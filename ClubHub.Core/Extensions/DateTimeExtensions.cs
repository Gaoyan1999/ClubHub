namespace ClubHub.Core.Extensions;

public static class DateTimeExtensions
{
    /// <summary>True when the range [start, end) overlaps [otherStart, otherEnd).</summary>
    public static bool Overlaps(this DateTime start, DateTime end, DateTime otherStart, DateTime otherEnd)
        => start < otherEnd && otherStart < end;
}
