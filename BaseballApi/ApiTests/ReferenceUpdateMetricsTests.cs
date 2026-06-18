using System.Diagnostics.Metrics;
using BaseballApi.Observability;

namespace BaseballApiTests;

/// <summary>
/// Verifies the MLBAM reference-update metrics (the player-match outcome counter and the post-run
/// match-state gauges) emit through the shared <see cref="Telemetry.MeterName"/> meter. Like the
/// other metrics tests these use an in-process <see cref="MeterListener"/> rather than a real
/// exporter, so they need no collector or database. Each test builds its own
/// <see cref="ReferenceUpdateMetrics"/> from a <see cref="TestMeterFactory"/>.
/// </summary>
public class ReferenceUpdateMetricsTests
{
    [Fact]
    [Trait(TestCategory.Category, TestCategory.Observability)]
    public void RecordPlayerMatchOutcome_IncrementsCounterWithOutcomeTag()
    {
        using var meterFactory = new TestMeterFactory();
        var metrics = new ReferenceUpdateMetrics(meterFactory);

        var measurements = new List<(long Value, string? Outcome)>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == Telemetry.MeterName && instrument.Name == "reference_update.player_match")
                {
                    l.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((_, value, tags, _) =>
        {
            string? outcome = null;
            foreach (var tag in tags)
            {
                if (tag.Key == "outcome")
                {
                    outcome = tag.Value as string;
                }
            }
            measurements.Add((value, outcome));
        });
        listener.Start();

        metrics.RecordPlayerMatchOutcome(ReferenceUpdateMetrics.PlayerMatchOutcome.Ambiguous);

        // Contains rather than Single: a parallel test could record its own outcome on another
        // ReferenceUpdateMetrics instance while this listener is active (same shared meter name).
        Assert.Contains(measurements, m => m.Value == 1 && m.Outcome == "ambiguous");
    }

    [Fact]
    [Trait(TestCategory.Category, TestCategory.Observability)]
    public void PlayersGauge_ReportsLatestSnapshotSplitByMatched()
    {
        using var meterFactory = new TestMeterFactory();
        var metrics = new ReferenceUpdateMetrics(meterFactory);
        // Distinctive counts so they aren't confused with snapshots other test classes may have
        // published on the shared meter.
        metrics.UpdatePlayerStateSnapshot(new ReferenceUpdateMetrics.PlayerStateSnapshot(
            Matched: 4242,
            Unmatched: 1717,
            AmbiguousNameGroups: 0,
            FixableUnmatched: 0));

        var measurements = new List<(long Value, bool? Matched)>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == Telemetry.MeterName && instrument.Name == "reference_update.players")
                {
                    l.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((_, value, tags, _) =>
        {
            bool? matched = null;
            foreach (var tag in tags)
            {
                if (tag.Key == "matched")
                {
                    matched = tag.Value as bool?;
                }
            }
            measurements.Add((value, matched));
        });
        listener.Start();

        listener.RecordObservableInstruments();

        Assert.Contains(measurements, m => m.Value == 4242 && m.Matched == true);
        Assert.Contains(measurements, m => m.Value == 1717 && m.Matched == false);
    }

    [Fact]
    [Trait(TestCategory.Category, TestCategory.Observability)]
    public void ScalarGauges_ReportLatestSnapshotValues()
    {
        using var meterFactory = new TestMeterFactory();
        var metrics = new ReferenceUpdateMetrics(meterFactory);
        // Distinctive values so they aren't confused with snapshots other test classes may have
        // published on the shared meter (hence Contains rather than an exact lookup below).
        metrics.UpdatePlayerStateSnapshot(new ReferenceUpdateMetrics.PlayerStateSnapshot(
            Matched: 0,
            Unmatched: 0,
            AmbiguousNameGroups: 91,
            FixableUnmatched: 73));

        var measurements = new List<(string Instrument, long Value)>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == Telemetry.MeterName
                    && (instrument.Name == "reference_update.ambiguous_name_groups"
                        || instrument.Name == "reference_update.fixable_unmatched"))
                {
                    l.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((instrument, value, _, _) => measurements.Add((instrument.Name, value)));
        listener.Start();

        listener.RecordObservableInstruments();

        Assert.Contains(("reference_update.ambiguous_name_groups", 91L), measurements);
        Assert.Contains(("reference_update.fixable_unmatched", 73L), measurements);
    }
}
