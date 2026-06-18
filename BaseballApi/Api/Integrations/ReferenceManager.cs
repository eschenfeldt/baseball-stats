using System.Diagnostics;
using BaseballApi.Models;
using BaseballApi.Observability;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Integrations;

public class ReferenceManager(ILogger<ReferenceManager> logger, BaseballContext context, IMLBAMConnector mlbamConnector, FangraphsConnector fangraphsConnector, ReferenceUpdateMetrics metrics)
{
    private ILogger<ReferenceManager> Logger { get; } = logger;
    private IMLBAMConnector MLBAMConnector { get; } = mlbamConnector;
    private BaseballContext Context { get; } = context;
    private FangraphsConnector FangraphsConnector { get; } = fangraphsConnector;
    private ReferenceUpdateMetrics Metrics { get; } = metrics;

    public async Task<int> UpdateTeamReferences(CancellationToken cancellation)
    {
        var mlbamTeamsResponse = await MLBAMConnector.GetTeamsAsync(cancellation);
        Logger.LogInformation("Fetched {count} teams from MLBAM.", mlbamTeamsResponse.Teams.Count);
        var updatedCount = 0;
        foreach (var mlbamTeam in mlbamTeamsResponse.Teams)
        {
            var team = Context.Teams.FirstOrDefault(t => t.MLBAMId == mlbamTeam.Id);
            if (team == null)
            {
                team = Context.Teams.FirstOrDefault(t => t.Name == mlbamTeam.TeamName && t.City == mlbamTeam.FranchiseName);
                if (team != null)
                {
                    team.MLBAMId = mlbamTeam.Id;
                    Logger.LogInformation("Updated MLBAMId for team {team} to {mlbamId}.", team.Name, mlbamTeam.Id);
                    updatedCount++;
                }
            }
        }
        await Context.SaveChangesAsync(cancellation);
        Logger.LogInformation("Updated {updatedCount} team references from MLBAM.", updatedCount);
        return updatedCount;
    }

    public async Task<PlayerReferenceUpdateResult> UpdatePlayerReferences(CancellationToken cancellation)
    {
        var mlbamCurrentPlayers = await MLBAMConnector.GetPlayersAsync(cancellation);
        int createdCount = 0;
        int updatedCount = 0;
        // Per-run disposition of each feed player, plus the colliding Players for each
        // ambiguous name, so we can both count outcomes and surface the fixable rows at the end.
        var outcomeCounts = new Dictionary<string, int>();
        var ambiguousByName = new Dictionary<string, List<Player>>();
        void RecordOutcome(string outcome)
        {
            Metrics.RecordPlayerMatchOutcome(outcome);
            outcomeCounts[outcome] = outcomeCounts.GetValueOrDefault(outcome) + 1;
        }
        Logger.LogInformation("Fetched {count} current players from MLBAM.", mlbamCurrentPlayers.People.Count);
        foreach (var mlbamPlayer in mlbamCurrentPlayers.People)
        {
            var isNewlyCreated = false;
            var referencePlayer = Context.ReferencePlayers
                .FirstOrDefault(rp => rp.MLBAMId == mlbamPlayer.Id);
            if (referencePlayer == null && !string.IsNullOrEmpty(mlbamPlayer.BirthDate))
            {
                var dob = DateOnly.Parse(mlbamPlayer.BirthDate);
                referencePlayer = Context.ReferencePlayers
                    .FirstOrDefault(rp => rp.Name == mlbamPlayer.FullName
                        && rp.DateOfBirth == dob);
                if (referencePlayer == null)
                {
                    referencePlayer = new ReferencePlayer
                    {
                        Name = mlbamPlayer.FullName,
                        DateOfBirth = dob,
                        MLBAMId = mlbamPlayer.Id
                    };
                    Context.ReferencePlayers.Add(referencePlayer);
                    isNewlyCreated = true;
                    createdCount++;
                    Logger.LogInformation("Created ReferencePlayer for {player} with MLBAMId {mlbamId}.", mlbamPlayer.FullName, mlbamPlayer.Id);
                }
            }
            if (referencePlayer == null)
            {
                // No MLBAMId match and no birthdate to match or create on: nothing we can do
                // this run. Previously a silent fall-through; now counted explicitly.
                RecordOutcome(ReferenceUpdateMetrics.PlayerMatchOutcome.DroppedNoBirthdate);
            }
            else
            {
                var updated = false;
                if (referencePlayer.MLBAMId != mlbamPlayer.Id)
                {
                    if (referencePlayer.MLBAMId.HasValue)
                    {
                        Logger.LogWarning("ReferencePlayer {player} has mismatched MLBAMId: existing '{oldId}', new '{mlbamId}'.",
                            referencePlayer.Name, referencePlayer.MLBAMId, mlbamPlayer.Id);
                    }
                    else
                    {
                        Logger.LogInformation("Setting MLBAMId for ReferencePlayer {player} to '{mlbamId}'.",
                            referencePlayer.Name, mlbamPlayer.Id);
                    }
                    referencePlayer.MLBAMId = mlbamPlayer.Id;
                    updated = true;
                }
                if (!string.IsNullOrEmpty(mlbamPlayer.PrimaryNumber)
                        && int.TryParse(mlbamPlayer.PrimaryNumber, out var number)
                        && referencePlayer.CurrentNumber != number)
                {
                    Logger.LogInformation("Updating current number for ReferencePlayer {player} from '{oldNumber}' to '{number}'.",
                        referencePlayer.Name, referencePlayer.CurrentNumber, number);
                    referencePlayer.CurrentNumber = number;
                    updated = true;
                }
                var team = Context.Teams.FirstOrDefault(t => t.MLBAMId == mlbamPlayer.CurrentTeam.Id);
                if (team != null && referencePlayer.CurrentTeamId != team.Id)
                {
                    Logger.LogInformation("Updating current team for ReferencePlayer {player} to '{team}': previously {oldTeam}.",
                        referencePlayer.Name, team.Name, referencePlayer.CurrentTeam?.Name ?? "none");
                    referencePlayer.CurrentTeamId = team.Id;
                    updated = true;
                }
                var match = MatchPlayerByMLBAMPlayer(referencePlayer, mlbamPlayer);
                RecordOutcome(match.Outcome);
                if (match.Outcome == ReferenceUpdateMetrics.PlayerMatchOutcome.Ambiguous && match.AmbiguousPlayers != null)
                {
                    ambiguousByName[mlbamPlayer.FullName.ToLowerInvariant()] = match.AmbiguousPlayers;
                }
                if (match.Player != null && referencePlayer.PlayerId != match.Player.Id)
                {
                    Logger.LogInformation("Linking ReferencePlayer {referencePlayer} to Player {player} (id {playerId}): previously {oldPlayer} (id {oldPlayerId}).",
                        referencePlayer.Name, match.Player.Name, match.Player.Id,
                        referencePlayer.Player?.Name ?? "none", referencePlayer.PlayerId);
                    referencePlayer.Player = match.Player;
                    updated = true;
                }
                if (updated && !isNewlyCreated)
                {
                    updatedCount++;
                }
            }
            var totalProcessed = createdCount + updatedCount;
            if (totalProcessed > 0 && totalProcessed % 100 == 0)
            {
                await Context.SaveChangesAsync(cancellation);
                Logger.LogInformation("Saved progress: {createdCount} players created, {updatedCount} players updated so far.",
                    createdCount, updatedCount);
            }
        }
        await Context.SaveChangesAsync(cancellation);
        Logger.LogInformation("Completed player reference update: {createdCount} players created, {updatedCount} players updated.",
            createdCount, updatedCount);

        await RecordPlayerMatchState(outcomeCounts, ambiguousByName, cancellation);

        // when we have more integrations they can also be handled here
        // unless they require too many api calls

        return new PlayerReferenceUpdateResult
        {
            CreatedCount = createdCount,
            UpdatedCount = updatedCount
        };
    }

    /// <summary>
    /// Cap on how many ambiguous name groups / unmatched Players we spell out individually in the
    /// end-of-run logs; counts are always reported in full, only the row-level detail is sampled.
    /// </summary>
    private const int MaxAmbiguousGroupsLogged = 50;
    private const int MaxUnmatchedSampleLogged = 25;

    /// <summary>
    /// Captures the post-run match state for the observability gauges and surfaces the actual
    /// fixable rows (ambiguous name groups) plus a sample of unmatched Players in the logs. The DB
    /// counts are queried fresh rather than derived from the loop so they reflect total state, not
    /// just this run's feed.
    /// </summary>
    private async Task RecordPlayerMatchState(
        IReadOnlyDictionary<string, int> outcomeCounts,
        IReadOnlyDictionary<string, List<Player>> ambiguousByName,
        CancellationToken cancellation)
    {
        var totalPlayers = await Context.Players.CountAsync(cancellation);
        var matchedPlayers = await Context.Players
            .CountAsync(p => Context.ReferencePlayers.Any(rp => rp.PlayerId == p.Id), cancellation);
        var unmatchedPlayers = totalPlayers - matchedPlayers;
        var ambiguousGroups = ambiguousByName.Count;
        // Only the genuinely unmatched members of the ambiguous groups are "fixable". A group can
        // also contain a Player that's already linked to a ReferencePlayer (e.g. one linked by name
        // only before a second Player with the same name appeared); counting those would overstate
        // the fixable-unmatched figure and break its "Unmatched tracked Players" definition.
        var ambiguousPlayerIds = ambiguousByName.Values
            .SelectMany(players => players)
            .Select(p => p.Id)
            .Distinct()
            .ToList();
        var fixableUnmatched = await Context.Players
            .CountAsync(p => ambiguousPlayerIds.Contains(p.Id)
                && !Context.ReferencePlayers.Any(rp => rp.PlayerId == p.Id), cancellation);

        Metrics.UpdatePlayerStateSnapshot(new ReferenceUpdateMetrics.PlayerStateSnapshot(
            Matched: matchedPlayers,
            Unmatched: unmatchedPlayers,
            AmbiguousNameGroups: ambiguousGroups,
            FixableUnmatched: fixableUnmatched));

        var activity = Activity.Current;
        if (activity != null)
        {
            foreach (var (outcome, count) in outcomeCounts)
            {
                activity.SetTag($"match.{outcome}", count);
            }
            activity.SetTag("match.players_total", totalPlayers);
            activity.SetTag("match.players_matched", matchedPlayers);
            activity.SetTag("match.players_unmatched", unmatchedPlayers);
            activity.SetTag("match.ambiguous_name_groups", ambiguousGroups);
        }

        Logger.LogInformation(
            "Player match summary: {linkedByNameAndDob} linked by name+DOB, {linkedByNameOnly} by name only, {ambiguous} ambiguous, {unmatched} unmatched, {droppedNoBirthdate} dropped (no birthdate). DB state: {matchedPlayers}/{totalPlayers} Players matched, {unmatchedPlayers} unmatched, {fixableUnmatched} fixable across {ambiguousGroups} ambiguous name groups.",
            outcomeCounts.GetValueOrDefault(ReferenceUpdateMetrics.PlayerMatchOutcome.LinkedByNameAndDob),
            outcomeCounts.GetValueOrDefault(ReferenceUpdateMetrics.PlayerMatchOutcome.LinkedByNameOnly),
            outcomeCounts.GetValueOrDefault(ReferenceUpdateMetrics.PlayerMatchOutcome.Ambiguous),
            outcomeCounts.GetValueOrDefault(ReferenceUpdateMetrics.PlayerMatchOutcome.Unmatched),
            outcomeCounts.GetValueOrDefault(ReferenceUpdateMetrics.PlayerMatchOutcome.DroppedNoBirthdate),
            matchedPlayers, totalPlayers, unmatchedPlayers, fixableUnmatched, ambiguousGroups);

        foreach (var (name, players) in ambiguousByName.Take(MaxAmbiguousGroupsLogged))
        {
            Logger.LogWarning(
                "Ambiguous match for feed name '{name}': {count} tracked Players share it and none could be uniquely linked; set DOBs to disambiguate. Candidates: {candidates}",
                name, players.Count, DescribePlayers(players));
        }
        if (ambiguousGroups > MaxAmbiguousGroupsLogged)
        {
            Logger.LogInformation("{remaining} additional ambiguous name groups not individually logged.",
                ambiguousGroups - MaxAmbiguousGroupsLogged);
        }

        if (unmatchedPlayers > 0)
        {
            // Newest Players first (highest Id): a Player gets created when a current-season game is
            // imported, so the most recently added unmatched rows are the ones most likely to be a
            // real gap worth chasing rather than expected MiLB/historical noise.
            var sample = await Context.Players
                .Where(p => !Context.ReferencePlayers.Any(rp => rp.PlayerId == p.Id))
                .OrderByDescending(p => p.Id)
                .Take(MaxUnmatchedSampleLogged)
                .ToListAsync(cancellation);
            Logger.LogInformation(
                "{unmatchedPlayers} tracked Players have no ReferencePlayer (much of this is expected: the feed is current-season MLB only). {sampleCount} most recently added shown: {sample}",
                unmatchedPlayers, sample.Count, DescribePlayers(sample));
        }
    }

    private static string DescribePlayers(IEnumerable<Player> players) =>
        string.Join("; ", players.Select(p => $"#{p.Id} {p.Name} (DOB {p.DateOfBirth?.ToString() ?? "none"})"));

    /// <summary>
    /// Limit number of players to process in each batch to avoid overwhelming Fangraphs
    /// </summary>
    private static readonly int FangraphsBatchSize = 100;

    public async Task<FangraphsLinkUpdateResult> UpdateFangraphsLinks(CancellationToken cancellation)
    {
        var players = Context.Players
            .Where(p => p.FangraphsPage == null)
            .OrderBy(p => p.Id)
            .Take(FangraphsBatchSize)
            .ToList();
        int playerUpdateCount = 0;
        int referencePlayerUpdateCount = 0;
        foreach (var player in players)
        {
            var fangraphsUrl = await FangraphsConnector.FindFangraphsPageForPlayer(player, cancellation);
            if (fangraphsUrl != null)
            {
                player.FangraphsPage = fangraphsUrl;
                playerUpdateCount++;
                Logger.LogInformation("Updated Fangraphs page for Player {player} to {fangraphsUrl}.",
                    player.Name, fangraphsUrl);
                var referencePlayer = Context.ReferencePlayers
                    .FirstOrDefault(rp => rp.PlayerId == player.Id);
                if (referencePlayer != null)
                {
                    var fangraphsId = FangraphsIdFromUri(fangraphsUrl);
                    if (referencePlayer.FangraphsId != null && referencePlayer.FangraphsId != fangraphsId)
                    {
                        Logger.LogWarning("ReferencePlayer {referencePlayer} already has FangraphsId {fangraphsId}; overwriting with {newFangraphsId}.",
                            referencePlayer.Name, referencePlayer.FangraphsId,
                            fangraphsId);
                    }
                    referencePlayer.FangraphsId = fangraphsId;
                    referencePlayerUpdateCount++;
                    Logger.LogInformation("Updated FangraphsId for ReferencePlayer {referencePlayer} to {fangraphsId}.",
                        referencePlayer.Name, fangraphsId);
                }
            }
        }
        await Context.SaveChangesAsync(cancellation);
        if (players.Count < FangraphsBatchSize)
        {
            Logger.LogInformation("Only {count} Players without Fangraphs links found to update, checking for reference players without ID set.", players.Count);
            var referencePlayersWithoutIds = Context.ReferencePlayers
                .Where(rp => rp.FangraphsId == null && rp.PlayerId != null);

            if (referencePlayersWithoutIds.Any())
            {
                var toUpdate = referencePlayersWithoutIds
                    .Include(rp => rp.Player)
                    .OrderBy(rp => rp.Id)
                    .Take(FangraphsBatchSize).ToList();
                Logger.LogInformation("Found {count} ReferencePlayers without FangraphsIds linked to Players; attempting to update Fangraphs IDs.",
                    toUpdate.Count);
                foreach (var referencePlayer in toUpdate)
                {
                    if (referencePlayer.Player == null)
                    {
                        Logger.LogWarning("ReferencePlayer {referencePlayer} has no linked Player; skipping fangraphs ID update.",
                            referencePlayer.Name);
                        continue;
                    }
                    var fangraphsUrl = await FangraphsConnector.FindFangraphsPageForPlayer(referencePlayer.Player, cancellation);
                    if (fangraphsUrl != null)
                    {
                        var fangraphsId = FangraphsIdFromUri(fangraphsUrl);
                        if (referencePlayer.FangraphsId != null && referencePlayer.FangraphsId != fangraphsId)
                        {
                            Logger.LogWarning("ReferencePlayer {referencePlayer} already has FangraphsId {fangraphsId}; overwriting with {newFangraphsId}.",
                                referencePlayer.Name, referencePlayer.FangraphsId,
                                fangraphsId);
                        }
                        Logger.LogInformation("Updating FangraphsId for ReferencePlayer {referencePlayer} to {fangraphsId}.",
                            referencePlayer.Name, fangraphsId);
                        referencePlayer.FangraphsId = fangraphsId;
                        referencePlayerUpdateCount++;
                    }
                }
                Logger.LogInformation("Updated {updatedCount} ReferencePlayers with new Fangraphs IDs.", referencePlayerUpdateCount);
            }
        }
        else
        {
            Logger.LogInformation("Saved {updatedCount} Players with new Fangraphs links.", playerUpdateCount);
        }
        return new FangraphsLinkUpdateResult
        {
            PlayersUpdated = playerUpdateCount,
            ReferencePlayersUpdated = referencePlayerUpdateCount
        };
    }

    /// <summary>
    /// Result of attempting to link a feed player to a tracked <see cref="Player"/>. <see cref="Outcome"/>
    /// is one of the <see cref="ReferenceUpdateMetrics.PlayerMatchOutcome"/> constants; <see cref="AmbiguousPlayers"/>
    /// carries the colliding Players when <see cref="Outcome"/> is
    /// <see cref="ReferenceUpdateMetrics.PlayerMatchOutcome.Ambiguous"/>, for end-of-run reporting.
    /// </summary>
    private readonly record struct PlayerMatch(Player? Player, string Outcome, List<Player>? AmbiguousPlayers);

    private PlayerMatch MatchPlayerByMLBAMPlayer(ReferencePlayer referencePlayer, MLBAMPlayer mlbamPlayer)
    {
        // First match by name and date of birth. NameNormalized is unaccent_immutable(lower(Name)),
        // which already strips the diacritics that DB Player names can carry; the MLBAM feed delivers
        // names without diacritics, so lowering the feed name is enough to line the two up.
        var referenceName = mlbamPlayer.FullName.ToLowerInvariant();
        var player = Context.Players.FirstOrDefault(p => p.NameNormalized == referenceName && p.DateOfBirth == referencePlayer.DateOfBirth);
        if (player != null)
        {
            Logger.LogInformation("Matched MLBAM player {mlbamPlayer} to Player {player} by name and date of birth.",
                mlbamPlayer.FullName, player.Name);
            return new PlayerMatch(player, ReferenceUpdateMetrics.PlayerMatchOutcome.LinkedByNameAndDob, null);
        }
        // Next look for a unique match by name only
        var playersByName = Context.Players.Where(p => p.NameNormalized == referenceName).ToList();
        if (playersByName.Count == 1)
        {
            Logger.LogInformation("Matched MLBAM player {mlbamPlayer} to Player {player} by name only.",
                mlbamPlayer.FullName, playersByName.First().Name);
            return new PlayerMatch(playersByName.First(), ReferenceUpdateMetrics.PlayerMatchOutcome.LinkedByNameOnly, null);
        }
        else if (playersByName.Count > 1)
        {
            // Multiple name matches: can't pick one. These are the actionable cases (set DOBs to
            // disambiguate); they're collected and logged together at the end of the run.
            return new PlayerMatch(null, ReferenceUpdateMetrics.PlayerMatchOutcome.Ambiguous, playersByName);
        }

        return new PlayerMatch(null, ReferenceUpdateMetrics.PlayerMatchOutcome.Unmatched, null);
    }

    private static string FangraphsIdFromUri(Uri uri)
    {
        var segments = uri.Segments;
        if (segments.Length < 4)
        {
            throw new ArgumentException("Invalid Fangraphs player URI: " + uri.ToString());
        }
        return segments[3].TrimEnd('/');
    }
}
