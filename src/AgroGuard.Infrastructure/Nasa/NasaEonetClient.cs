using System.Text.Json;
using AgroGuard.Application.Abstractions;

namespace AgroGuard.Infrastructure.Nasa;

internal sealed class NasaEonetClient : INasaEventClient
{
    private readonly HttpClient _httpClient;

    public NasaEonetClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<NasaNaturalEvent>> GetOpenEventsAsync(
        int days,
        string? category,
        CancellationToken cancellationToken)
    {
        var safeDays = Math.Clamp(days, 1, 365);
        var requestUri = $"events?status=open&limit=50&days={safeDays}";

        if (!string.IsNullOrWhiteSpace(category))
        {
            requestUri += $"&category={Uri.EscapeDataString(category.Trim())}";
        }

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("events", out var eventsElement))
        {
            return [];
        }

        return eventsElement
            .EnumerateArray()
            .Select(ReadEvent)
            .Where(eventData => eventData is not null)
            .Cast<NasaNaturalEvent>()
            .ToArray();
    }

    private static NasaNaturalEvent? ReadEvent(JsonElement eventElement)
    {
        var id = ReadString(eventElement, "id");
        var title = ReadString(eventElement, "title");

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var categoryId = "unknown";
        var categoryName = "Evento natural";
        if (eventElement.TryGetProperty("categories", out var categories) && categories.GetArrayLength() > 0)
        {
            var category = categories[0];
            categoryId = ReadString(category, "id") ?? categoryId;
            categoryName = ReadString(category, "title") ?? categoryName;
        }

        DateTime? date = null;
        decimal? latitude = null;
        decimal? longitude = null;
        decimal? magnitude = null;
        string? magnitudeUnit = null;

        if (eventElement.TryGetProperty("geometry", out var geometries) && geometries.GetArrayLength() > 0)
        {
            var geometry = geometries[0];
            date = TryReadDate(ReadString(geometry, "date"));
            magnitude = ReadDecimal(geometry, "magnitudeValue");
            magnitudeUnit = ReadString(geometry, "magnitudeUnit");

            if (geometry.TryGetProperty("coordinates", out var coordinates) &&
                TryReadCoordinatePair(coordinates, out var lon, out var lat))
            {
                latitude = lat;
                longitude = lon;
            }
        }

        string? sourceUrl = null;
        if (eventElement.TryGetProperty("sources", out var sources) && sources.GetArrayLength() > 0)
        {
            sourceUrl = ReadString(sources[0], "url");
        }

        return new NasaNaturalEvent(
            id,
            title,
            categoryId,
            categoryName,
            date,
            latitude,
            longitude,
            magnitude,
            magnitudeUnit,
            sourceUrl);
    }

    private static bool TryReadCoordinatePair(JsonElement element, out decimal longitude, out decimal latitude)
    {
        longitude = 0m;
        latitude = 0m;

        if (element.ValueKind != JsonValueKind.Array || element.GetArrayLength() < 2)
        {
            return false;
        }

        if (element[0].ValueKind == JsonValueKind.Number && element[1].ValueKind == JsonValueKind.Number)
        {
            longitude = element[0].GetDecimal();
            latitude = element[1].GetDecimal();
            return latitude is >= -90m and <= 90m && longitude is >= -180m and <= 180m;
        }

        foreach (var child in element.EnumerateArray())
        {
            if (TryReadCoordinatePair(child, out longitude, out latitude))
            {
                return true;
            }
        }

        return false;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static decimal? ReadDecimal(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDecimal()
            : null;
    }

    private static DateTime? TryReadDate(string? value)
    {
        return DateTime.TryParse(value, out var date)
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : null;
    }
}
