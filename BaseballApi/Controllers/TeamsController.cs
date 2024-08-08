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
        public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
        {
            return await _context.Teams.ToListAsync();
        }

        [HttpGet("summaries")]
        public async Task<ActionResult<PagedResult<TeamSummary>>> GetTeamSummaries(int skip, int take, string? sort = null, bool asc = false)
        {
            TeamSummaryOrder order = sort.ToEnumOrDefault<TeamSummaryOrder, ParamValueAttribute>();

            var awayGames = _context.Teams
                .GroupJoin(_context.Games, t => t.Id, g => g.Away.Id, (team, games) => new
                {
                    Team = team,
                    Games = games.Count(),
                    Wins = games.Count(g => g.WinningTeam == team),
                    Losses = games.Count(g => g.LosingTeam == team),
                    LastGame = games.Select(g => g.Date).DefaultIfEmpty().Max()
                });
            var homeGames = _context.Teams
                .GroupJoin(_context.Games, t => t.Id, g => g.Home.Id, (team, games) => new
                {
                    Team = team,
                    Games = games.Count(),
                    Wins = games.Count(g => g.WinningTeam == team),
                    Losses = games.Count(g => g.LosingTeam == team),
                    LastGame = games.Select(g => g.Date).DefaultIfEmpty().Max()
                });
            var awayList = awayGames.ToList();
            var homeList = homeGames.ToList();
            var query = awayGames.Join(homeGames, ag => ag.Team, hg => hg.Team, (ag, hg) => new TeamSummary
            {
                Team = ag.Team ?? hg.Team,
                Games = ag.Games + hg.Games,
                Wins = ag.Wins + hg.Wins,
                Losses = ag.Losses + hg.Losses,
                LastGameDate = ag.LastGame > hg.LastGame ? ag.LastGame : hg.LastGame
            });
            var sorted = GetSorted(query, order, asc);
            return new PagedResult<TeamSummary>
            {
                TotalCount = await query.CountAsync(),
                Results = await sorted
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync()
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

        private static IOrderedQueryable<TeamSummary> GetSorted(IQueryable<TeamSummary> query, TeamSummaryOrder order, bool asc)
        {
            if (asc)
            {
                return order switch
                {
                    TeamSummaryOrder.Team => query.OrderBy(k => k.Team),
                    TeamSummaryOrder.Games => query.OrderBy(k => k.Games),
                    TeamSummaryOrder.Wins => query.OrderBy(k => k.Wins),
                    TeamSummaryOrder.Losses => query.OrderBy(k => k.Losses),
                    TeamSummaryOrder.LastGame => query.OrderBy(k => k.LastGameDate),
                    _ => query.OrderBy(k => k.Games),
                };
            }
            else
            {
                return order switch
                {
                    TeamSummaryOrder.Team => query.OrderByDescending(k => k.Team),
                    TeamSummaryOrder.Games => query.OrderByDescending(k => k.Games),
                    TeamSummaryOrder.Wins => query.OrderByDescending(k => k.Wins),
                    TeamSummaryOrder.Losses => query.OrderByDescending(k => k.Losses),
                    TeamSummaryOrder.LastGame => query.OrderByDescending(k => k.LastGameDate),
                    _ => query.OrderByDescending(k => k.Games),
                };
            }
        }
    }
}
