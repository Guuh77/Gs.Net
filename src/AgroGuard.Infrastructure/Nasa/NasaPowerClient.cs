using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using AgroGuard.Application.Abstractions;

namespace AgroGuard.Infrastructure.Nasa;

internal sealed class NasaPowerClient : INasaPowerClient
{
    private const string Parameters = "T2M,T2M_MAX,T2M_MIN,PRECTOTCORR,RH2M,GWETROOT,GWETTOP,CLOUD_AMT,ALLSKY_SFC_SW_DWN";
    private static readonly ConcurrentDictionary<string, SummaryCacheEntry> SummaryCache = new();
    private static readonly ConcurrentDictionary<string, SeriesCacheEntry> SeriesCache = new();
    private readonly HttpClient _httpClient;

    public NasaPowerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<NasaPowerClimateSummary> GetAgroClimateAsync(
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken)
    {
        var endDate = DateTime.UtcNow.Date.AddDays(-6);
        var startDate = endDate.AddDays(-6);
        var cacheKey = $"{latitude:0.0000}:{longitude:0.0000}:{startDate:yyyyMMdd}:{endDate:yyyyMMdd}";

        if (SummaryCache.TryGetValue(cacheKey, out var cacheEntry) && cacheEntry.ExpiresAt > DateTime.UtcNow)
        {
            return cacheEntry.Value;
        }

        var series = await FetchClimateSeriesAsync(latitude, longitude, startDate, endDate, cancellationToken);
        SummaryCache[cacheKey] = new SummaryCacheEntry(series.Summary, DateTime.UtcNow.AddMinutes(20));
        return series.Summary;
    }

    public async Task<NasaPowerClimateSeries> GetClimateSeriesAsync(
        decimal latitude,
        decimal longitude,
        int days,
        CancellationToken cancellationToken)
    {
        var safeDays = Math.Clamp(days, 7, 60);
        var endDate = DateTime.UtcNow.Date.AddDays(-6);
        var startDate = endDate.AddDays(-(safeDays - 1));
        var cacheKey = $"{latitude:0.0000}:{longitude:0.0000}:{startDate:yyyyMMdd}:{endDate:yyyyMMdd}:series";

        if (SeriesCache.TryGetValue(cacheKey, out var cacheEntry) && cacheEntry.ExpiresAt > DateTime.UtcNow)
        {
            return cacheEntry.Value;
        }

        var series = await FetchClimateSeriesAsync(latitude, longitude, startDate, endDate, cancellationToken);
        SeriesCache[cacheKey] = new SeriesCacheEntry(series, DateTime.UtcNow.AddMinutes(20));
        return series;
    }

    private async Task<NasaPowerClimateSeries> FetchClimateSeriesAsync(
        decimal latitude,
        decimal longitude,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(BuildRequestUri(latitude, longitude, startDate, endDate), cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var root = document.RootElement;
        var parameter = root.GetProperty("properties").GetProperty("parameter");
        var header = root.GetProperty("header");
        var daily = ReadDailyClimate(parameter).ToArray();

        var headerStartDate = ParsePowerDate(header.GetProperty("start").GetString()!);
        var headerEndDate = ParsePowerDate(header.GetProperty("end").GetString()!);
        var summary = BuildSummary(headerStartDate, headerEndDate, daily);

        return new NasaPowerClimateSeries(summary, daily);
    }

    private static NasaPowerClimateSummary BuildSummary(
        DateTime startDate,
        DateTime endDate,
        IReadOnlyCollection<NasaPowerDailyClimate> daily)
    {
        return new NasaPowerClimateSummary(
            "NASA POWER Daily API",
            startDate,
            endDate,
            Average(daily.Select(day => day.TemperatureCelsius)),
            Max(daily.Select(day => day.MaximumTemperatureCelsius)),
            Sum(daily.Select(day => day.RainfallMillimeters)),
            Average(daily.Select(day => day.RelativeHumidityPercent)),
            Average(daily.Select(day => day.RootZoneSoilWetness)),
            Average(daily.Select(day => day.SurfaceSoilWetness)),
            Average(daily.Select(day => day.CloudCoveragePercent)));
    }

    private static IEnumerable<NasaPowerDailyClimate> ReadDailyClimate(JsonElement parameter)
    {
        if (!parameter.TryGetProperty("T2M", out var temperatureByDate))
        {
            yield break;
        }

        foreach (var day in temperatureByDate.EnumerateObject())
        {
            var temperature = ReadValue(parameter, "T2M", day.Name);
            if (temperature is null)
            {
                continue;
            }

            yield return new NasaPowerDailyClimate(
                ParsePowerDate(day.Name),
                temperature.Value,
                ReadValue(parameter, "T2M_MAX", day.Name) ?? temperature.Value,
                ReadValue(parameter, "T2M_MIN", day.Name) ?? temperature.Value,
                ReadValue(parameter, "PRECTOTCORR", day.Name) ?? 0m,
                ReadValue(parameter, "RH2M", day.Name) ?? 0m,
                ReadValue(parameter, "ALLSKY_SFC_SW_DWN", day.Name) ?? 0m,
                ReadValue(parameter, "GWETROOT", day.Name) ?? 0m,
                ReadValue(parameter, "GWETTOP", day.Name) ?? 0m,
                ReadValue(parameter, "CLOUD_AMT", day.Name) ?? 0m);
        }
    }

    private static decimal? ReadValue(JsonElement parameter, string parameterName, string date)
    {
        if (!parameter.TryGetProperty(parameterName, out var values) ||
            !values.TryGetProperty(date, out var valueElement))
        {
            return null;
        }

        var value = valueElement.GetDecimal();
        return value > -900m ? value : null;
    }

    private static string BuildRequestUri(decimal latitude, decimal longitude, DateTime startDate, DateTime endDate)
    {
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);
        var start = startDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var end = endDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        return $"api/temporal/daily/point?parameters={Parameters}&community=AG&longitude={lon}&latitude={lat}&start={start}&end={end}&format=JSON";
    }

    private static DateTime ParsePowerDate(string value)
    {
        return DateTime.SpecifyKind(
            DateTime.ParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture),
            DateTimeKind.Utc);
    }

    private static decimal Average(IEnumerable<decimal> values)
    {
        var validValues = values.ToArray();
        return validValues.Length == 0
            ? 0m
            : decimal.Round(validValues.Average(), 2, MidpointRounding.AwayFromZero);
    }

    private static decimal Max(IEnumerable<decimal> values)
    {
        var validValues = values.ToArray();
        return validValues.Length == 0 ? 0m : validValues.Max();
    }

    private static decimal Sum(IEnumerable<decimal> values)
    {
        return decimal.Round(values.Sum(), 2, MidpointRounding.AwayFromZero);
    }

    private sealed record SummaryCacheEntry(NasaPowerClimateSummary Value, DateTime ExpiresAt);

    private sealed record SeriesCacheEntry(NasaPowerClimateSeries Value, DateTime ExpiresAt);
}
