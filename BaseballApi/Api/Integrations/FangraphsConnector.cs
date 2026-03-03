using System.Text.Json;
using BaseballApi.Models;

namespace BaseballApi.Integrations;

public class FangraphsConnector(HttpClient httpClient)
{
    public async Task<Uri?> FindFangraphsPageForPlayer(Player player, CancellationToken cancellation)
    {
        var searchResult = await SearchPlayerByName(player.Name, cancellation);
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
            // Use normalized name but fall back to name in case player hasn't been saved
            var playerName = player.NameNormalized ?? player.Name;
            // multiple matches - first check for single exact name match
            var exactNameMatches = searchResult.Hits
                .Where(fgPlayer => fgPlayer.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase))
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

    private async Task<FangraphsPlayerSearchResult> SearchPlayerByName(string name, CancellationToken cancellation)
    {
        var requestUri = new UriBuilder(httpClient.BaseAddress!)
        {
            Query = $"search={Uri.EscapeDataString(name)}"
        };
        var response = await httpClient.GetAsync(requestUri.Uri, cancellation);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellation);
        return JsonSerializer.Deserialize<FangraphsPlayerSearchResult>(content);
    }
}
