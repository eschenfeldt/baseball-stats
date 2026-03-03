using BaseballApi.Contracts;
using BaseballApi.Models;

namespace BaseballApi.Import;

public class PlayerManager(BaseballContext context)
{
    private BaseballContext Context { get; } = context;
    private Dictionary<string, Player> NewPlayers { get; } = [];

    public Player GetOrCreatePlayer(string name, long teamId, int number, int? year)
    {
        List<ReferencePlayer> referenceMatches = [];
        var dbMatches = Context.Players
            .Where(p => p.Name == name).ToList();
        var useReferenceData = year == null || year == DateTime.Now.Year;
        if (useReferenceData)
        {
            // use reference data for the current year
            // Scope by just name first
            referenceMatches = Context.ReferencePlayers
                .Where(rp => rp.Name == name)
                .ToList();
            if (referenceMatches.Count == 1)
            {
                // single match, should be good to use
                var referencePlayer = referenceMatches.First();
                return GetPlayerFromReference(name, referencePlayer, dbMatches);
            }
            else if (referenceMatches.Count > 1)
            {
                // multiple matches - try to disambiguate by team and number
                foreach (var referencePlayer in referenceMatches)
                {
                    if (referencePlayer.CurrentTeam != null && referencePlayer.CurrentTeam.Id == teamId
                        && referencePlayer.CurrentNumber.HasValue && referencePlayer.CurrentNumber.Value == number)
                    {
                        return GetPlayerFromReference(name, referencePlayer, dbMatches);
                    }
                }
                // still ambiguous - try by team only
                foreach (var referencePlayer in referenceMatches)
                {
                    if (referencePlayer.CurrentTeam != null && referencePlayer.CurrentTeam.Id == teamId)
                    {
                        return GetPlayerFromReference(name, referencePlayer, dbMatches);
                    }
                }
                // still ambiguous - fall back to db-based approaches
            }
        }

        if (dbMatches.Count == 0)
        {
            return CreatePlayer(name);
        }
        else if (dbMatches.Count == 1)
        {
            return dbMatches.First();
        }
        else
        {
            // multiple players with same name - try to disambiguate by team and year first
            foreach (var player in dbMatches)
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
            foreach (var player in dbMatches)
            {
                var playedOnTeam = ConstructPlayerGamesQuery(player.Id, Context.Games, teamId)
                    .Any();
                if (playedOnTeam)
                {
                    return player;
                }
            }
            // still ambiguous - if looking at current year go by first reference matches if available, otherwise first db match
            // Eventually we may have retrosheet data to check prior years in reference data
            if (useReferenceData && referenceMatches.Count > 0)
            {
                return GetPlayerFromReference(name, referenceMatches.First(), dbMatches);
            }
            else
            {
                return dbMatches.First();
            }
        }
    }

    private Player GetPlayerFromReference(string name, ReferencePlayer referencePlayer, List<Player> dbMatches)
    {
        if (referencePlayer.Player != null)
        {
            return referencePlayer.Player;
        }
        else if (dbMatches.Count == 1)
        {
            // don't depend on reference data link being set correctly if unambiguous in db
            // but we don't want to set the link here; depend on the official matching processes for that
            return dbMatches.First();
        }
        else
        {
            return CreatePlayer(name, referencePlayer);
        }
    }

    private Player CreatePlayer(string name, ReferencePlayer? referencePlayer = null)
    {
        if (!NewPlayers.TryGetValue(name, out var newPlayer))
        {
            newPlayer = new Player
            {
                Name = name
            };
            this.NewPlayers[name] = newPlayer;
        }

        if (referencePlayer != null)
        {
            referencePlayer.Player = newPlayer;
        }
        return newPlayer;
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
