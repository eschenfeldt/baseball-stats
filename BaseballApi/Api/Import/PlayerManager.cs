using System;
using BaseballApi.Models;

namespace BaseballApi.Import;

public class PlayerManager(BaseballContext context)
{
    private BaseballContext Context { get; } = context;
    private Dictionary<string, Player> NewPlayers { get; } = [];

    public Player GetOrCreatePlayer(string name, long teamId, int year)
    {
        var matches = Context.Players
            .Where(p => p.Name == name).ToList();

        if (matches.Count == 0)
        {
            if (NewPlayers.TryGetValue(name, out var existingNewPlayer))
            {
                return existingNewPlayer;
            }
            else
            {
                var newPlayer = new Player
                {
                    Name = name
                };
                this.NewPlayers[name] = newPlayer;
                return newPlayer;
            }
        }
        else if (matches.Count == 1)
        {
            return matches.First();
        }
        else
        {
            // multiple players with same name - try to disambiguate by team and year first
            foreach (var player in matches)
            {
                var playedOnTeamInYear = ConstructPlayerGamesQuery(player.Id, Context.Games, teamId)
                    .Where(g => g.Date.Year == year)
                    .Any();
                if (playedOnTeamInYear)
                {
                    return player;
                }
            }
            // try by team only
            foreach (var player in matches)
            {
                var playedOnTeam = ConstructPlayerGamesQuery(player.Id, Context.Games, teamId)
                    .Any();
                if (playedOnTeam)
                {
                    return player;
                }
            }
            // still ambiguous - just return first match
            // TODO: search Fangraphs stats to match team outside of known games
            return matches.First();
        }
    }

    public async Task<Uri> FindFangraphsPageForPlayer(Player player)
    {
        throw new NotImplementedException();
    }

    public static IQueryable<Game> ConstructPlayerGamesQuery(long playerId, IQueryable<Game> baseGames, long? teamId = null)
    {
        return baseGames
            .Where(g => (
                (teamId == null || g.Away.Id == teamId)
                && g.AwayBoxScore != null && (
                g.AwayBoxScore.Batters.Any(b => b.PlayerId == playerId)
                || g.AwayBoxScore.Pitchers.Any(p => p.PlayerId == playerId)
                || g.AwayBoxScore.Fielders.Any(f => f.PlayerId == playerId)
            )) || (
                (teamId == null || g.Home.Id == teamId)
                && g.HomeBoxScore != null && (
                g.HomeBoxScore.Batters.Any(b => b.PlayerId == playerId)
                || g.HomeBoxScore.Pitchers.Any(p => p.PlayerId == playerId)
                || g.HomeBoxScore.Fielders.Any(f => f.PlayerId == playerId)
            )));
    }
}
