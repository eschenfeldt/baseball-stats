using System.Text;
using BaseballApi.Contracts;
using BaseballApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly BaseballContext _context;

        public SearchController(BaseballContext context)
        {
            _context = context;
        }

        [HttpGet("{searchQuery}")]
        public async Task<ActionResult<IEnumerable<SearchResult>>> Search(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return BadRequest("Search query cannot be empty.");
            }

            var results = new List<SearchResult>();
            string ilikeQuery = $"%{searchQuery}%";

            // Search for teams
            var teams = await _context.Teams
                .Where(t => EF.Functions.ILike(t.City, ilikeQuery) ||
                            EF.Functions.ILike(t.Name, ilikeQuery) ||
                            EF.Functions.ILike(t.City + " " + t.Name, ilikeQuery) ||
                            (t.Abbreviation != null && EF.Functions.ILike(t.Abbreviation, ilikeQuery)) ||
                            t.AlternateTeamNames.Any(a => EF.Functions.ILike(a.FullName, ilikeQuery)))
                .ToListAsync();

            foreach (var team in teams)
            {
                results.Add(new SearchResult
                {
                    Name = $"{team.City} {team.Name}",
                    Description = team.Abbreviation ?? "",
                    Type = SearchResultType.Team,
                    Id = team.Id
                });
            }

            // Search for players
            var players = await _context.Players
                .Where(p => EF.Functions.ILike(p.Name, ilikeQuery))
                .Take(10)
                .ToListAsync();

            foreach (var player in players)
            {
                results.Add(new SearchResult
                {
                    Name = player.Name,
                    Description = await GetPlayerDescription(player),
                    Type = SearchResultType.Player,
                    Id = player.Id
                });
            }

            return results;
        }

        private async Task<string> GetPlayerDescription(Player player)
        {
            var awayGames = await _context.Games.Where(g => g.AwayBoxScore != null && (
                                    g.AwayBoxScore.Batters.Any(b => b.PlayerId == player.Id)
                                    || g.AwayBoxScore.Pitchers.Any(p => p.PlayerId == player.Id)
                                    || g.AwayBoxScore.Fielders.Any(f => f.PlayerId == player.Id)
                                    ))
                                    .Select(g => new
                                    {
                                        Team = g.Away.Abbreviation ?? g.Away.City + " " + g.Away.Name,
                                        g.Date.Year
                                    }).Distinct().ToListAsync();

            var homeGames = await _context.Games.Where(g => g.HomeBoxScore != null && (
                                    g.HomeBoxScore.Batters.Any(b => b.PlayerId == player.Id)
                                    || g.HomeBoxScore.Pitchers.Any(p => p.PlayerId == player.Id)
                                    || g.HomeBoxScore.Fielders.Any(f => f.PlayerId == player.Id)
                                    ))
                                    .Select(g => new
                                    {
                                        Team = g.Home.Abbreviation ?? g.Home.City + " " + g.Home.Name,
                                        g.Date.Year
                                    }).Distinct().ToListAsync();
            var teamYears = awayGames.Union(homeGames)
                .GroupBy(g => g.Team)
                .Select(g => new
                {
                    Team = g.Key,
                    MinYear = g.Min(x => x.Year),
                    Years = g.Select(x => x.Year).Distinct()
                }).OrderBy(g => g.MinYear);

            var description = new StringBuilder();
            foreach (var teamYear in teamYears)
            {
                if (description.Length > 0)
                {
                    description.Append(", ");
                }
                description.Append($"{teamYear.Team} {DisplayYears(teamYear.Years)}");
            }
            return description.ToString();
        }

        private static string DisplayYears(IEnumerable<int> years)
        {
            if (years == null || !years.Any())
            {
                return "";
            }

            var minYear = years.Min().ToString().Substring(2, 2);
            var maxYear = years.Max().ToString().Substring(2, 2);

            if (minYear == maxYear)
            {
                return $"{minYear}";
            }
            else
            {
                return $"{minYear}-{maxYear}";
            }
        }
    }
}
