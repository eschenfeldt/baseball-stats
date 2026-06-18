using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BaseballApi.Observability;

/// <summary>
/// Metrics for the media-import pipeline: the final-outcome counter recorded by the background
/// service and the queue-depth gauge fed by the import queue. Registered as a DI singleton so its
/// instruments are created exactly once, under the shared <see cref="Telemetry.MeterName"/> meter.
/// </summary>
public sealed class MediaImportMetrics
{
    private readonly Counter<long> _outcomes;

    // Supplies the current queue depth to the observable gauge. Written once when the queue is
    // constructed, read by the metrics export thread; see TrackQueueDepth.
    private Func<int>? _queueDepth;

    public MediaImportMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(Telemetry.MeterName);
        _outcomes = meter.CreateCounter<long>(
            "media_import.outcomes",
            unit: "{import}",
            description: "Media import runs tagged by their final outcome.");
        // Early-warning signal that the importer is falling behind, pulled on each collection.
        meter.CreateObservableGauge(
            "media_import.queue.depth",
            ObserveQueueDepth,
            unit: "{import}",
            description: "Media imports waiting in the queue to be processed.");
    }

    /// <summary>Final outcome values for the media-import counter and span tag.</summary>
    public static class Outcome
    {
        public const string Completed = "completed";
        public const string Failed = "failed";
        public const string Skipped = "skipped";
    }

    /// <summary>
    /// Records a finished media import: increments the outcome counter tagged by
    /// <paramref name="outcome"/> (use the <see cref="Outcome"/> constants) and, when a job span is
    /// active, annotates it so a trace can be filtered by the same outcome. A crash mid-import is
    /// intentionally not an outcome here — that surfaces as a span error instead. The span tag
    /// rides on <see cref="Activity.Current"/>, so call this while the job's activity is ambient.
    /// </summary>
    public void RecordOutcome(string outcome)
    {
        _outcomes.Add(1, new KeyValuePair<string, object?>("status", outcome));
        Activity.Current?.SetTag("import.outcome", outcome);
    }

    /// <summary>
    /// Binds the queue-depth gauge to its data source. Call once when the queue is constructed;
    /// until then the gauge reports nothing rather than a misleading zero.
    /// </summary>
    public void TrackQueueDepth(Func<int> queueDepth) => Volatile.Write(ref _queueDepth, queueDepth);

    private IEnumerable<Measurement<int>> ObserveQueueDepth() =>
        Volatile.Read(ref _queueDepth) is { } depth ? [new Measurement<int>(depth())] : [];
}
