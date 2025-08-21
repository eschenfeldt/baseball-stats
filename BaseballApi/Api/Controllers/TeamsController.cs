using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaseballApi;
using BaseballApi.Models;
using BaseballApi.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BaseballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly BaseballContext _context;

        public TeamsController(BaseballContext context)
        {
            _context = context;
        }

        // GET: api/Teams
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetTeams(long? parkId = null, int? year = null)
        {
            IQueryable<Game> games = _context.Games;
            if (parkId.HasValue)
            {
                games = games.Where(g => g.LocationId == parkId);
            }
            if (year.HasValue)
            {
                games = games.Where(g => g.Date.Year == year);
            }

            var awayTeams = _context.Teams.Join(games, t => t.Id, g => g.Away.Id, (team, games) => team);
            var homeTeams = _context.Teams.Join(games, t => t.Id, g => g.Home.Id, (team, games) => team);
            return await awayTeams.Union(homeTeams).OrderBy(t => t.City).ThenBy(t => t.Name).ToListAsync();
        }

        [HttpGet("summaries")]
        public ActionResult<PagedResult<TeamSummary>> GetTeamSummaries(int skip, int take, string? sort = null, bool asc = false)
        {
            TeamSummaryOrder order = sort.ToEnumOrDefault<TeamSummaryOrder, ParamValueAttribute>();

            var awayGames = _context.Teams
                .GroupJoin(_context.Games, t => t.Id, g => g.Away.Id, (team, games) => new
                {
                    Team = team,
                    Games = games.Count(),
                    Wins = games.Count(g => g.WinningTeam == team),
                    Losses = games.Count(g => g.LosingTeam == team),
                    LastGame = games.Select(g => g.Date).DefaultIfEmpty().Max(),
                    Parks = games.Select(g => g.LocationId).Distinct()
                });
            var homeGames = _context.Teams
                .GroupJoin(_context.Games, t => t.Id, g => g.Home.Id, (team, games) => new
                {
                    Team = team,
                    Games = games.Count(),
                    Wins = games.Count(g => g.WinningTeam == team),
                    Losses = games.Count(g => g.LosingTeam == team),
                    LastGame = games.Select(g => g.Date).DefaultIfEmpty().Max(),
                    Parks = games.Select(g => g.LocationId).Distinct()
                });
            var awayList = awayGames.ToList();
            var homeList = homeGames.ToList();
            var query = awayGames.Join(homeGames, ag => ag.Team, hg => hg.Team, (ag, hg) => new TeamSummary
            {
                Team = ag.Team ?? hg.Team,
                Games = ag.Games + hg.Games,
                Wins = ag.Wins + hg.Wins,
                Losses = ag.Losses + hg.Losses,
                LastGameDate = ag.LastGame > hg.LastGame ? ag.LastGame : hg.LastGame,
                Parks = ag.Parks.Union(hg.Parks).Count(id => id.HasValue)
            }).AsEnumerable();
            var sorted = GetSorted(query, order, asc);
            return new PagedResult<TeamSummary>
            {
                TotalCount = query.Count(),
                Results = sorted
                    .Skip(skip)
                    .Take(take)
                    .ToList()
            };
        }

        // GET: api/Teams/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Team>> GetTeam(long id)
        {
            var team = await _context.Teams.FindAsync(id);

            if (team == null)
            {
                return NotFound();
            }

            return team;
        }

        // PUT: api/Teams/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutTeam(long id, Team team)
        {
            if (id != team.Id)
            {
                return BadRequest();
            }

            _context.Entry(team).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeamExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Teams
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Team>> PostTeam(Team team)
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTeam", new { id = team.Id }, team);
        }

        // DELETE: api/Teams/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTeam(long id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TeamExists(long id)
        {
            return _context.Teams.Any(e => e.Id == id);
        }

        private static IOrderedEnumerable<TeamSummary> GetSorted(IEnumerable<TeamSummary> query, TeamSummaryOrder order, bool asc)
        {
            if (asc)
            {
                return order switch
                {
                    TeamSummaryOrder.Team => query.OrderBy(k => k.Team.City),
                    TeamSummaryOrder.Games => query.OrderBy(k => k.Games),
                    TeamSummaryOrder.Wins => query.OrderBy(k => k.Wins),
                    TeamSummaryOrder.Losses => query.OrderBy(k => k.Losses),
                    TeamSummaryOrder.LastGame => query.OrderBy(k => k.LastGameDate),
                    TeamSummaryOrder.Parks => query.OrderBy(k => k.Parks),
                    _ => query.OrderBy(k => k.Games),
                };
            }
            else
            {
                return order switch
                {
                    TeamSummaryOrder.Team => query.OrderByDescending(k => k.Team.City),
                    TeamSummaryOrder.Games => query.OrderByDescending(k => k.Games),
                    TeamSummaryOrder.Wins => query.OrderByDescending(k => k.Wins),
                    TeamSummaryOrder.Losses => query.OrderByDescending(k => k.Losses),
                    TeamSummaryOrder.LastGame => query.OrderByDescending(k => k.LastGameDate),
                    TeamSummaryOrder.Parks => query.OrderByDescending(k => k.Parks),
                    _ => query.OrderByDescending(k => k.Games),
                };
            }
        }
    }
}
