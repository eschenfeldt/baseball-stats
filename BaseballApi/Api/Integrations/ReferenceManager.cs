using System;
using BaseballApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Integrations;

public class ReferenceManager(ILogger<ReferenceManager> logger, BaseballContext context, IMLBAMConnector mlbamConnector) : IDisposable
{
    private ILogger<ReferenceManager> Logger { get; } = logger;
    private IMLBAMConnector MLBAMConnector { get; } = mlbamConnector;
    private BaseballContext Context { get; } = context;

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
        // get players from MLBAM who are on a current roster
        // update or create ReferencePlayer entries as needed
        // Return number of players updated/created
        var mlbamCurrentPlayers = await MLBAMConnector.GetPlayersAsync(cancellation);
        int createdCount = 0;
        int updatedCount = 0;
        Logger.LogInformation("Fetched {count} current players from MLBAM.", mlbamCurrentPlayers.People.Count);
        foreach (var mlbamPlayer in mlbamCurrentPlayers.People)
        {
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
                if (updated)
                {
                    updatedCount++;
                }
            }
            if (createdCount + updatedCount % 100 == 0)
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
            UpdatedCount = updatedCount - createdCount
        };
    }

    private Player? MatchPlayerByMLBAMPlayer(ReferencePlayer referencePlayer, MLBAMPlayer mlbamPlayer)
    {
        // First match by name and date of birth
        var player = Context.Players.FirstOrDefault(p => EF.Functions.Unaccent(p.Name) == EF.Functions.Unaccent(mlbamPlayer.FullName) && p.DateOfBirth == referencePlayer.DateOfBirth);
        if (player != null)
        {
            Logger.LogInformation("Matched MLBAM player {mlbamPlayer} to Player {player} by name and date of birth.",
                mlbamPlayer.FullName, player.Name);
            return player;
        }
        // Next look for a unique match by name only
        var playersByName = Context.Players.Where(p => EF.Functions.Unaccent(p.Name) == EF.Functions.Unaccent(mlbamPlayer.FullName)).ToList();
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

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}
