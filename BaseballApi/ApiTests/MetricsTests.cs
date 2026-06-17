using System.Diagnostics.Metrics;
using BaseballApi.Observability;
using BaseballApi.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace BaseballApiTests;

/// <summary>
/// Verifies the custom metrics (import-outcome counter + queue-depth gauge) emit through
/// the shared <see cref="Telemetry.Meter"/>. Like the span tests these use an in-process
/// listener rather than a real exporter, so they need no collector or database.
/// </summary>
public class MetricsTests
{
    [Fact]
    [Trait(TestCategory.Category, TestCategory.Observability)]
    public void RecordMediaImportOutcome_IncrementsCounterWithStatusTag()
    {
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

        Telemetry.RecordMediaImportOutcome(Telemetry.MediaImportOutcome.Failed);

        var measurement = Assert.Single(measurements);
        Assert.Equal(1, measurement.Value);
        Assert.Equal("failed", measurement.Status);
    }

    [Fact]
    [Trait(TestCategory.Category, TestCategory.Observability)]
    public async Task QueueDepthGauge_ReportsCurrentQueueCount()
    {
        var queue = new MediaImportQueue(NullLogger<MediaImportQueue>.Instance);
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
