namespace ClubHub.Core.Services;

/// <summary>Rain during an event: the highest hourly chance of rain and the total rain expected.</summary>
public record RainForecast(int MaxChancePercent, double TotalMm)
{
    /// <summary>The admin is warned at this chance of rain or higher.</summary>
    public const int WarningChancePercent = 50;

    public bool IsRainLikely => MaxChancePercent >= WarningChancePercent;
}
