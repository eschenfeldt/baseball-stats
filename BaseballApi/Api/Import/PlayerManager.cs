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
            .Where(p => p.Name == name);

        if (!matches.Any())
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
                Context.Players.Add(newPlayer);
                Context.SaveChanges();
                return newPlayer;
            }
        }
        else if (matches.Count() == 1)
        {
            return matches.First();
        }
        else
        {
            // multiple players with same name - try to disambiguate by team and year first
            foreach (var player in matches)
            {
                var playedInTeamYear = Context.BoxScorePlayers
                    .Any(bsp => bsp.PlayerId == player.Id && bsp.BoxScore.TeamId == teamId && bsp.BoxScore.Game.Date.Year == year);
                if (playedInTeamYear)
                {
                    return player;
                }
            }
            // try by team only
            foreach (var player in matches)
            {
                var playedInTeamYear = Context.BoxScorePlayers
                    .Any(bsp => bsp.PlayerId == player.Id && bsp.BoxScore.TeamId == teamId);
                if (playedInTeamYear)
                {
                    return player;
                }
            }
            // still ambiguous - just return first match
            // TODO: search Fangraphs stats to match team outside of known games
            return matches.First();
        }
    }

    public string FindFangraphsPageForPlayer(Player player)
    {
        throw new NotImplementedException();
    }
}
