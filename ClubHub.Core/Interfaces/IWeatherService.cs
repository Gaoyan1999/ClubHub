using ClubHub.Core.Services;

namespace ClubHub.Core.Interfaces;

public interface IWeatherService
{
    /// <summary>
    /// Rain forecast at UTS for the given time. Null when there is no forecast:
    /// the time is too far ahead, there is no internet, or the weather service fails.
    /// </summary>
    Task<RainForecast?> GetRainForecastAsync(DateTime start, DateTime end);
}
