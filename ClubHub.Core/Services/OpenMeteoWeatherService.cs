using System.Globalization;
using System.Text.Json;
using ClubHub.Core.Extensions;
using ClubHub.Core.Interfaces;

namespace ClubHub.Core.Services;

/// <summary>
/// Gets hourly rain forecasts from Open-Meteo (free, no API key). It only covers about the next
/// 16 days. Every failure returns null, so the app works without internet.
/// </summary>
public class OpenMeteoWeatherService : IWeatherService
{
    // UTS city campus, Ultimo. All club rooms are on campus.
    private const double Latitude = -33.8832;
    private const double Longitude = 151.2005;

    private readonly HttpClient _http;

    public OpenMeteoWeatherService() : this(new HttpClient { Timeout = TimeSpan.FromSeconds(5) })
    {
    }

    // Tests pass in an HttpClient with a fake handler
    public OpenMeteoWeatherService(HttpClient http)
    {
        _http = http;
    }

    public async Task<RainForecast?> GetRainForecastAsync(DateTime start, DateTime end)
    {
        // Event times are Sydney local time, so ask for the forecast in Sydney time too
        var url = "https://api.open-meteo.com/v1/forecast"
                  + FormattableString.Invariant($"?latitude={Latitude}&longitude={Longitude}")
                  + "&hourly=precipitation_probability,precipitation&timezone=Australia%2FSydney"
                  + FormattableString.Invariant($"&start_date={start:yyyy-MM-dd}&end_date={end:yyyy-MM-dd}");

        try
        {
            var json = await _http.GetStringAsync(url);
            return ParseForecast(json, start, end);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // No internet, timeout, or a date outside the forecast range (the API answers 400)
            return null;
        }
    }

    /// <summary>
    /// Reads the hours that overlap the event from an Open-Meteo response.
    /// Null when the response has no usable hours for that time.
    /// </summary>
    public static RainForecast? ParseForecast(string json, DateTime start, DateTime end)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("hourly", out var hourly))
                return null;

            var times = hourly.GetProperty("time").EnumerateArray()
                .Select(t => DateTime.Parse(t.GetString()!, CultureInfo.InvariantCulture))
                .ToList();
            var chances = ReadNumbers(hourly, "precipitation_probability");
            var amounts = ReadNumbers(hourly, "precipitation");

            // Each value covers one hour; keep the hours that overlap the event and have a chance value
            var hours = Enumerable.Range(0, times.Count)
                .Where(i => times[i].Overlaps(times[i].AddHours(1), start, end) && chances.ElementAtOrDefault(i) is not null)
                .ToList();
            if (hours.Count == 0)
                return null;

            return new RainForecast(
                MaxChancePercent: (int)hours.Max(i => chances[i]!.Value),
                TotalMm: Math.Round(hours.Sum(i => amounts.ElementAtOrDefault(i) ?? 0), 1));
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or FormatException or InvalidOperationException)
        {
            return null;
        }
    }

    // Open-Meteo uses null for hours it has no value for
    private static List<double?> ReadNumbers(JsonElement hourly, string name) =>
        hourly.GetProperty(name).EnumerateArray()
              .Select(v => v.ValueKind == JsonValueKind.Number ? v.GetDouble() : (double?)null)
              .ToList();
}
