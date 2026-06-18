using System.Diagnostics.Metrics;

namespace BaseballApi.Observability;

/// <summary>
/// Metrics for the MLBAM reference-update job: a per-feed-player match-outcome counter plus the
/// post-run match-state gauges. Registered as a DI singleton so its instruments are created
/// exactly once, under the shared <see cref="Telemetry.MeterName"/> meter.
/// </summary>
public sealed class ReferenceUpdateMetrics
{
    private readonly Counter<long> _playerMatch;

    // Latest match-state snapshot, published by the reference-update job and read by the metrics
    // export thread. Accessed through Volatile.Read/Write — see UpdatePlayerStateSnapshot for why.
    private PlayerStateSnapshot? _playerState;

    public ReferenceUpdateMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(Telemetry.MeterName);
        _playerMatch = meter.CreateCounter<long>(
            "reference_update.player_match",
            unit: "{player}",
            description: "Per-run disposition of each MLBAM feed player when matching it to a tracked Player.");
        meter.CreateObservableGauge(
            "reference_update.players",
            ObservePlayers,
            unit: "{player}",
            description: "Tracked Players split by whether they link to a ReferencePlayer, as of the last reference update run.");
        meter.CreateObservableGauge(
            "reference_update.ambiguous_name_groups",
            () => Observe(static s => s.AmbiguousNameGroups),
            description: "Distinct normalized names that the last run could not uniquely match because multiple Players share them.");
        meter.CreateObservableGauge(
            "reference_update.fixable_unmatched",
            () => Observe(static s => s.FixableUnmatched),
            unit: "{player}",
            description: "Unmatched tracked Players that appeared in the feed but could not be disambiguated; fixable by setting a DOB.");
    }

    /// <summary>
    /// Final disposition of a single MLBAM feed player during a reference update — each feed
    /// player is counted under exactly one of these. <see cref="DroppedNoBirthdate"/> never even
    /// gets a ReferencePlayer; the rest reflect whether/how its ReferencePlayer maps to a tracked
    /// <c>Player</c>. <see cref="Ambiguous"/> is the actionable failure (fix by adding a DOB).
    /// </summary>
    public static class PlayerMatchOutcome
    {
        public const string LinkedByNameAndDob = "linked_by_name_and_dob";
        public const string LinkedByNameOnly = "linked_by_name_only";
        public const string Ambiguous = "ambiguous";
        public const string Unmatched = "unmatched";
        public const string DroppedNoBirthdate = "dropped_no_birthdate";
    }

    /// <summary>
    /// Snapshot of tracked-Player match state captured at the end of a reference update run.
    /// <see cref="FixableUnmatched"/> is the count of Players in this run's ambiguous-name groups
    /// — unmatched only because the feed player couldn't be disambiguated.
    /// </summary>
    public sealed record PlayerStateSnapshot(
        long Matched,
        long Unmatched,
        long AmbiguousNameGroups,
        long FixableUnmatched);

    /// <summary>
    /// Increments the player-match counter for one feed player, tagged by its
    /// <paramref name="outcome"/> (use the <see cref="PlayerMatchOutcome"/> constants). Per-run
    /// totals are summarized onto the job span separately by the caller.
    /// </summary>
    public void RecordPlayerMatchOutcome(string outcome)
    {
        _playerMatch.Add(1, new KeyValuePair<string, object?>("outcome", outcome));
    }

    /// <summary>
    /// Publishes the latest player match-state snapshot for the observable gauges to report; call
    /// once at the end of a reference update run.
    ///
    /// The job thread writes here while the OpenTelemetry export thread reads in the gauge
    /// callbacks, so the field needs cross-thread publication. We use <see cref="Volatile.Write"/> /
    /// <see cref="Volatile.Read"/> on a plain field rather than the <c>volatile</c> keyword (whose
    /// implicit per-access semantics the C# docs now steer away from) or a lock (heavier than
    /// needed on the hot export path). This is correct <em>only because the snapshot is immutable
    /// and swapped wholesale</em>: a reference write is atomic, so a reader observes either the
    /// complete previous snapshot or the complete new one, never a torn mix of old and new counts.
    /// <see cref="Volatile.Write"/> supplies the release fence that makes the fully-constructed
    /// record visible; <see cref="Volatile.Read"/> the matching acquire. We need only eventual
    /// visibility ("state as of the last completed run"), which this provides; an
    /// <c>Interlocked.Exchange</c> would also work but we use neither its returned value nor its
    /// full barrier.
    /// </summary>
    public void UpdatePlayerStateSnapshot(PlayerStateSnapshot snapshot) =>
        Volatile.Write(ref _playerState, snapshot);

    private IEnumerable<Measurement<long>> ObservePlayers()
    {
        var state = Volatile.Read(ref _playerState);
        if (state is null)
        {
            yield break;
        }
        yield return new Measurement<long>(state.Matched, new KeyValuePair<string, object?>("matched", true));
        yield return new Measurement<long>(state.Unmatched, new KeyValuePair<string, object?>("matched", false));
    }

    private IEnumerable<Measurement<long>> Observe(Func<PlayerStateSnapshot, long> selector)
    {
        var state = Volatile.Read(ref _playerState);
        return state is null ? [] : [new Measurement<long>(selector(state))];
    }
}
