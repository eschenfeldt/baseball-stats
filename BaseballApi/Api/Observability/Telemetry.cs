using System.Diagnostics;

namespace BaseballApi.Observability;

/// <summary>
/// Shared, cross-cutting telemetry plumbing: the background-job <see cref="ActivitySource"/>, the
/// common job-exception helper, and the OpenTelemetry meter name that every per-subsystem metrics
/// class creates its instruments under. The source and meter name are registered with the
/// OpenTelemetry pipeline in <c>Program.cs</c> via <c>AddSource(Telemetry.BackgroundJobsSourceName)</c>
/// and <c>AddMeter(Telemetry.MeterName)</c>; without those registrations
/// <see cref="ActivitySource.StartActivity(string, ActivityKind)"/> returns null, so span call
/// sites must null-condition the returned activity.
///
/// Metrics deliberately do NOT live here. Each subsystem owns a dedicated, DI-registered metrics
/// class (e.g. <c>MediaImportMetrics</c>, <c>ReferenceUpdateMetrics</c>) holding its own
/// instruments, tag constants, and any cached gauge state, so this stays a thin shared core
/// rather than a grab-bag that grows with every new metric.
/// </summary>
public static class Telemetry
{
    public const string BackgroundJobsSourceName = "BaseballApi.BackgroundJobs";

    public static readonly ActivitySource BackgroundJobs = new(BackgroundJobsSourceName);

    /// <summary>
    /// Starts a background-job root span. Jobs are <see cref="ActivityKind.Consumer"/> rather than
    /// the default Internal: each run consumes a trigger (a queue item or a timer tick), and —
    /// decisively — Grafana Cloud's server-side span-metrics generation only covers SERVER and
    /// CONSUMER spans by default, so Internal job spans are invisible to the background-jobs
    /// dashboard and to Application Observability's operations view.
    /// </summary>
    public static Activity? StartJob(string name) => BackgroundJobs.StartActivity(name, ActivityKind.Consumer);

    /// <summary>
    /// Meter name shared by every custom metric. Per-subsystem metrics classes pass this to
    /// <c>IMeterFactory.Create</c>, and <c>Program.cs</c> registers it with <c>AddMeter</c>.
    /// </summary>
    public const string MeterName = "BaseballApi";

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
