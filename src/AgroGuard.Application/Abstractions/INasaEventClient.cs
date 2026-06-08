namespace AgroGuard.Application.Abstractions;

public interface INasaEventClient
{
    Task<IReadOnlyList<NasaNaturalEvent>> GetOpenEventsAsync(
        int days,
        string? category,
        CancellationToken cancellationToken);
}

public sealed record NasaNaturalEvent(
    string Id,
    string Title,
    string CategoryId,
    string CategoryName,
    DateTime? Date,
    decimal? Latitude,
    decimal? Longitude,
    decimal? Magnitude,
    string? MagnitudeUnit,
    string? SourceUrl);
