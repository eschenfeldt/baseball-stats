using BaseballApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Integrations;

public class ReferenceManager(ILogger<ReferenceManager> logger, BaseballContext context, IMLBAMConnector mlbamConnector, FangraphsConnector fangraphsConnector)
{
    private ILogger<ReferenceManager> Logger { get; } = logger;
    private IMLBAMConnector MLBAMConnector { get; } = mlbamConnector;
    private BaseballContext Context { get; } = context;
    private FangraphsConnector FangraphsConnector { get; } = fangraphsConnector;

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
            if (referencePlayer != null)
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
                var player = MatchPlayerByMLBAMPlayer(referencePlayer, mlbamPlayer);
                if (player != null && referencePlayer.PlayerId != player.Id)
                {
                    Logger.LogInformation("Linking ReferencePlayer {referencePlayer} to Player {player} (id {playerId}): previously {oldPlayer} (id {oldPlayerId}).",
                        referencePlayer.Name, player.Name, player.Id,
                        referencePlayer.Player?.Name ?? "none", referencePlayer.PlayerId);
                    referencePlayer.Player = player;
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

        // when we have more integrations they can also be handled here
        // unless they require too many api calls

        return new PlayerReferenceUpdateResult
        {
            CreatedCount = createdCount,
            UpdatedCount = updatedCount
        };
    }

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

    private Player? MatchPlayerByMLBAMPlayer(ReferencePlayer referencePlayer, MLBAMPlayer mlbamPlayer)
    {
        // First match by name and date of birth
        var referenceName = mlbamPlayer.FullName.ToLowerInvariant();
        var player = Context.Players.FirstOrDefault(p => p.NameNormalized == referenceName && p.DateOfBirth == referencePlayer.DateOfBirth);
        if (player != null)
        {
            Logger.LogInformation("Matched MLBAM player {mlbamPlayer} to Player {player} by name and date of birth.",
                mlbamPlayer.FullName, player.Name);
            return player;
        }
        // Next look for a unique match by name only
        var playersByName = Context.Players.Where(p => p.NameNormalized == referenceName).ToList();
        if (playersByName.Count == 1)
        {
            Logger.LogInformation("Matched MLBAM player {mlbamPlayer} to Player {player} by name only.",
                mlbamPlayer.FullName, playersByName.First().Name);
            return playersByName.First();
        }
        else if (playersByName.Count > 1)
        {
            // if there are multiple name matches, just warn for now;
            // probably we should be setting the dob on the player to disambiguate
            Logger.LogWarning("Warning: multiple players found with name {mlbamPlayerFullName}; cannot match by MLBAMId {mlbamPlayerId}", mlbamPlayer.FullName, mlbamPlayer.Id);
        }

        return null;
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
