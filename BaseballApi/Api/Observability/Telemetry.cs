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

    /// <summary>
    /// Flags a background-job span as failed for an exception, except for cancellation:
    /// that's process shutdown rather than a job failure, so recording it would make the
    /// span-metrics report a spurious error. Null-safe so call sites needn't check the
    /// (possibly null) activity themselves.
    /// </summary>
    public static void RecordJobException(Activity? activity, Exception ex)
    {
        if (ex is OperationCanceledException)
        {
            return;
        }
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.AddException(ex);
    }
}
