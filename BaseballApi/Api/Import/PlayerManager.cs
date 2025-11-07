using System;
using System.Text.Json;
using BaseballApi.Contracts;
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

    public async Task<Uri?> FindFangraphsPageForPlayer(Player player)
    {
        var searchResult = await SearchFangraphsPlayerByName(player.Name);
        if (searchResult.Hits.Count == 0)
        {
            return null;
        }
        else if (searchResult.Hits.Count == 1)
        {
            return FangraphsPlayerUri(searchResult.Hits[0]);
        }
        else
        {
            // multiple matches - first check for single exact name match
            var exactNameMatches = searchResult.Hits
                .Where(fgPlayer => fgPlayer.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (exactNameMatches.Count == 1)
            {
                return FangraphsPlayerUri(exactNameMatches[0]);
            }
            // next check date of birth if available
            if (player.DateOfBirth.HasValue)
            {
                foreach (var fgPlayer in searchResult.Hits)
                {
                    if (fgPlayer.BirthDate.HasValue && DateOnly.FromDateTime(fgPlayer.BirthDate.Value) == player.DateOfBirth.Value)
                    {
                        return FangraphsPlayerUri(fgPlayer);
                    }
                }
            }
            // still ambiguous - return null
            return null;
        }
    }

    private static Uri FangraphsPlayerUri(FangraphsPlayer player)
    {
        return new Uri($"https://www.fangraphs.com{player.Url}");
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

    private static readonly Uri FangraphsPlayerSearchUri = new("https://www.fangraphs.com/api/search/players/");

    private async Task<FangraphsPlayerSearchResult> SearchFangraphsPlayerByName(string name)
    {
        var client = new HttpClient();
        var requestUri = new UriBuilder(FangraphsPlayerSearchUri)
        {
            Query = $"search={Uri.EscapeDataString(name)}"
        };
        var response = await client.GetAsync(requestUri.Uri);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<FangraphsPlayerSearchResult>(content);
    }
}
