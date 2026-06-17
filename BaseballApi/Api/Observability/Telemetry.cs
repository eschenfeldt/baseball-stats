using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BaseballApi.Observability;

/// <summary>
/// Shared telemetry primitives for background-job tracing and metrics. The
/// <see cref="ActivitySource"/> and <see cref="Meter"/> are registered with the
/// OpenTelemetry pipeline in <c>Program.cs</c> via
/// <c>AddSource(Telemetry.BackgroundJobsSourceName)</c> and
/// <c>AddMeter(Telemetry.MeterName)</c>; without those registrations
/// <see cref="ActivitySource.StartActivity(string, ActivityKind)"/> returns null, so all
/// span call sites must null-condition the returned activity.
/// </summary>
public static class Telemetry
{
    public const string BackgroundJobsSourceName = "BaseballApi.BackgroundJobs";

    public static readonly ActivitySource BackgroundJobs = new(BackgroundJobsSourceName);

    public const string MeterName = "BaseballApi";

    public static readonly Meter Meter = new(MeterName);

    private static readonly Counter<long> MediaImportOutcomeCounter = Meter.CreateCounter<long>(
        "media_import.outcomes",
        unit: "{import}",
        description: "Media import runs tagged by their final outcome.");

    /// <summary>Final outcome values for the media-import counter and span tag.</summary>
    public static class MediaImportOutcome
    {
        public const string Completed = "completed";
        public const string Failed = "failed";
        public const string Skipped = "skipped";
    }

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

    /// <summary>
    /// Records a finished media import: increments the outcome counter tagged by
    /// <paramref name="outcome"/> (use the <see cref="MediaImportOutcome"/> constants) and, when a
    /// job span is active, annotates it so a trace can be filtered by the same outcome. A
    /// crash mid-import is intentionally not an outcome here — that surfaces as a span error
    /// instead. Note the span tag rides on <see cref="Activity.Current"/>, so call this while
    /// the job's activity is the ambient one.
    /// </summary>
    public static void RecordMediaImportOutcome(string outcome)
    {
        MediaImportOutcomeCounter.Add(1, new KeyValuePair<string, object?>("status", outcome));
        Activity.Current?.SetTag("import.outcome", outcome);
    }
}
