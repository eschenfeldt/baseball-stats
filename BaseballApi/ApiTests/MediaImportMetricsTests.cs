using System.Diagnostics.Metrics;
using BaseballApi.Observability;
using BaseballApi.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace BaseballApiTests;

/// <summary>
/// Verifies the media-import metrics (outcome counter + queue-depth gauge) emit through the shared
/// <see cref="Telemetry.MeterName"/> meter. Like the span tests these use an in-process listener
/// rather than a real exporter, so they need no collector or database. Each test builds its own
/// <see cref="MediaImportMetrics"/> from a <see cref="TestMeterFactory"/>.
/// </summary>
public class MediaImportMetricsTests
{
    [Fact]
    [Trait(TestCategory.Category, TestCategory.Observability)]
    public void RecordOutcome_IncrementsCounterWithStatusTag()
    {
        using var meterFactory = new TestMeterFactory();
        var metrics = new MediaImportMetrics(meterFactory);

        var measurements = new List<(long Value, string? Status)>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == Telemetry.MeterName && instrument.Name == "media_import.outcomes")
                {
                    l.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((_, value, tags, _) =>
        {
            string? status = null;
            foreach (var tag in tags)
            {
                if (tag.Key == "status")
                {
                    status = tag.Value as string;
                }
            }
            measurements.Add((value, status));
        });
        listener.Start();

        metrics.RecordOutcome(MediaImportMetrics.Outcome.Failed);

        // Contains rather than Single: a parallel test could record its own outcome on another
        // MediaImportMetrics instance while this listener is active (same shared meter name).
        Assert.Contains(measurements, m => m.Value == 1 && m.Status == "failed");
    }

    [Fact]
    [Trait(TestCategory.Category, TestCategory.Observability)]
    public async Task QueueDepthGauge_ReportsCurrentQueueCount()
    {
        using var meterFactory = new TestMeterFactory();
        var metrics = new MediaImportMetrics(meterFactory);
        // Constructing the queue binds the gauge to its count.
        var queue = new MediaImportQueue(NullLogger<MediaImportQueue>.Instance, metrics);
        // Three queued, none popped: a distinctive count so it's not confused with the
        // (typically empty) queues other test classes may hold alive on the shared meter.
        await queue.PushAsync(Guid.NewGuid());
        await queue.PushAsync(Guid.NewGuid());
        await queue.PushAsync(Guid.NewGuid());
        Assert.Equal(3, queue.Count);

        var observed = new List<int>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == Telemetry.MeterName && instrument.Name == "media_import.queue.depth")
                {
                    l.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<int>((_, value, _, _) => observed.Add(value));
        listener.Start();

        listener.RecordObservableInstruments();

        Assert.Contains(3, observed);
    }
}
