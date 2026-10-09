using System.Net;
using ClubHub.Core.Services;

namespace ClubHub.Tests;

public class WeatherServiceTests
{
    private static readonly DateTime EventStart = new(2026, 10, 17, 18, 0, 0);
    private static readonly DateTime EventEnd = new(2026, 10, 17, 21, 0, 0);

    // Shape of a real Open-Meteo response, cut down to the hours 17:00 to 22:00
    private const string Response = """
        {"hourly":{
          "time":["2026-10-17T17:00","2026-10-17T18:00","2026-10-17T19:00","2026-10-17T20:00","2026-10-17T21:00","2026-10-17T22:00"],
          "precipitation_probability":[95,40,70,null,90,90],
          "precipitation":[5.0,0.2,1.1,0.3,4.0,4.0]}}
        """;

    [Test]
    public void ParseForecast_UsesOnlyTheEventHours()
    {
        // 18:00, 19:00 and 20:00 overlap the event; 20:00 has no chance value so it is skipped
        var forecast = OpenMeteoWeatherService.ParseForecast(Response, EventStart, EventEnd);

        Assert.That(forecast, Is.EqualTo(new RainForecast(MaxChancePercent: 70, TotalMm: 1.3)));
        Assert.That(forecast!.IsRainLikely, Is.True);
    }

    [Test]
    public void IsRainLikely_StartsAtTheWarningChance()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new RainForecast(RainForecast.WarningChancePercent, 0).IsRainLikely, Is.True);
            Assert.That(new RainForecast(RainForecast.WarningChancePercent - 1, 0).IsRainLikely, Is.False);
        });
    }

    [TestCase("""{"reason":"Parameter 'start_date' is out of allowed range","error":true}""")]
    [TestCase("not json")]
    [TestCase("""{"hourly":{"time":["2026-10-18T18:00"],"precipitation_probability":[80],"precipitation":[1.0]}}""")]
    public void ParseForecast_IsNull_WhenThereIsNoUsableForecast(string json)
    {
        Assert.That(OpenMeteoWeatherService.ParseForecast(json, EventStart, EventEnd), Is.Null);
    }

    [Test]
    public async Task GetRainForecastAsync_ReadsTheResponse()
    {
        var service = new OpenMeteoWeatherService(new HttpClient(new FakeHandler(_ => Ok(Response))));

        var forecast = await service.GetRainForecastAsync(EventStart, EventEnd);

        Assert.That(forecast?.MaxChancePercent, Is.EqualTo(70));
    }

    [Test]
    public async Task GetRainForecastAsync_ReturnsNull_WhenOffline()
    {
        var service = new OpenMeteoWeatherService(new HttpClient(new FakeHandler(_ => throw new HttpRequestException("No internet"))));

        Assert.That(await service.GetRainForecastAsync(EventStart, EventEnd), Is.Null);
    }

    [Test]
    public async Task GetRainForecastAsync_ReturnsNull_WhenTheDateIsTooFarAhead()
    {
        // Open-Meteo answers 400 Bad Request for dates outside its forecast range
        var service = new OpenMeteoWeatherService(new HttpClient(new FakeHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("""{"error":true}""") })));

        Assert.That(await service.GetRainForecastAsync(EventStart.AddMonths(2), EventEnd.AddMonths(2)), Is.Null);
    }

    private static HttpResponseMessage Ok(string body) => new(HttpStatusCode.OK) { Content = new StringContent(body) };

    // Answers every request without going to the internet
    private class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }
}
