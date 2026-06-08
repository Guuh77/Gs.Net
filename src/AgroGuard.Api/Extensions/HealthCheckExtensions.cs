using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AgroGuard.Api.Extensions;

public static class HealthCheckExtensions
{
    public static HealthCheckOptions ToJsonResponse()
    {
        return new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    status = report.Status.ToString(),
                    totalDuration = report.TotalDuration.TotalMilliseconds,
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        duration = entry.Value.Duration.TotalMilliseconds,
                        description = entry.Value.Description
                    })
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        };
    }

    public static IHealthChecksBuilder AddSelfCheck(this IHealthChecksBuilder builder)
    {
        return builder.AddCheck("api", () => HealthCheckResult.Healthy("AgroGuard API is running."));
    }
}
