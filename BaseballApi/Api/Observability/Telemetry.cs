using System.Diagnostics;

namespace BaseballApi.Observability;

/// <summary>
/// Shared telemetry primitives for background-job tracing. The <see cref="ActivitySource"/>
/// is registered with the OpenTelemetry tracer in <c>Program.cs</c> via
/// <c>AddSource(Telemetry.BackgroundJobsSourceName)</c>; without that registration
/// <see cref="ActivitySource.StartActivity(string, ActivityKind)"/> returns null, so all
/// call sites must null-condition the returned activity.
/// </summary>
public static class Telemetry
{
    public const string BackgroundJobsSourceName = "BaseballApi.BackgroundJobs";

    public static readonly ActivitySource BackgroundJobs = new(BackgroundJobsSourceName);
}
