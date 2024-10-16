using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaseballApi;
using BaseballApi.Models;
using Microsoft.AspNetCore.Authorization;
using BaseballApi.Contracts;

namespace BaseballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly BaseballContext _context;

        public PlayerController(BaseballContext context)
        {
            _context = context;
        }

        // GET: api/Player
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            return await _context.Players.ToListAsync();
        }

        // GET: api/Player/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PlayerSummary>> GetPlayer(long id)
        {
            var player = await _context.Players.FindAsync(id);

            if (player == null)
            {
                return NotFound();
            }

            PlayerSummary summary = new()
            {
                Info = new(player),
                SummaryStats = []
            };

            var calculator = new StatCalculator(_context)
            {
                PlayerId = id
            };

            var aggregateBatting = await calculator.GetBattingStats().FirstOrDefaultAsync();
            if (aggregateBatting != null)
            {
                aggregateBatting.AddAsSummaryStats(summary.SummaryStats);
            }
            var aggregatePitching = await calculator.GetPitchingStats().FirstOrDefaultAsync();
            if (aggregatePitching != null)
            {
                aggregatePitching.AddAsSummaryStats(summary.SummaryStats);
            }

            return summary;
        }

        [HttpGet("games")]
        public async Task<ActionResult<PlayerGameResults>> GetGames(
            long playerId,
            int skip = 0,
            int take = 10,
            bool asc = false,
            int? year = null)
        {
            var query = this.GetPlayerGamesQuery(playerId)
                .Include(g => g.Home)
                .Include(g => g.Away)
                .Include(g => g.AwayBoxScore)
                    .ThenInclude(bs => bs.Batters)
                        .ThenInclude(p => p.Player)
                .Include(g => g.AwayBoxScore)
                    .ThenInclude(bs => bs.Pitchers)
                        .ThenInclude(p => p.Player)
                .Include(g => g.AwayBoxScore)
                    .ThenInclude(bs => bs.Fielders)
                        .ThenInclude(p => p.Player)
                .Include(g => g.HomeBoxScore)
                    .ThenInclude(bs => bs.Batters)
                        .ThenInclude(p => p.Player)
                .Include(g => g.HomeBoxScore)
                    .ThenInclude(bs => bs.Pitchers)
                        .ThenInclude(p => p.Player)
                .Include(g => g.HomeBoxScore)
                    .ThenInclude(bs => bs.Fielders)
                        .ThenInclude(p => p.Player)
                .AsSplitQuery();

            if (year.HasValue)
            {
                query = query.Where(g => g.Date.Year == year);
            }

            var sorted = asc ? query.OrderBy(g => g.StartTime ?? g.ScheduledTime ?? g.Date.ToDateTime(TimeOnly.MinValue))
                : query.OrderByDescending(g => g.StartTime ?? g.ScheduledTime ?? g.Date.ToDateTime(TimeOnly.MinValue));

            return new PlayerGameResults
            {
                TotalCount = await query.CountAsync(),
                Results = await sorted
                    .Select(g => new PlayerGame(g, playerId))
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync(),
                PitchingStats = StatCollection.GamePitchingStats,
                BattingStats = StatCollection.GameBattingStats
            };
        }

        [HttpGet("years")]
        public async Task<ActionResult<List<int>>> GetGameYears(long playerId)
        {
            IQueryable<Game> query = this.GetPlayerGamesQuery(playerId);

            return await query.Select(g => g.Date.Year).Distinct().OrderBy(i => i).ToListAsync();
        }

        private IQueryable<Game> GetPlayerGamesQuery(long playerId)
        {
            return _context.Games
                .Where(g => g.AwayBoxScore != null && (
                    g.AwayBoxScore.Batters.Any(b => b.PlayerId == playerId)
                    || g.AwayBoxScore.Pitchers.Any(p => p.PlayerId == playerId)
                    || g.AwayBoxScore.Fielders.Any(f => f.PlayerId == playerId)
                ) || g.HomeBoxScore != null && (
                    g.HomeBoxScore.Batters.Any(b => b.PlayerId == playerId)
                    || g.HomeBoxScore.Pitchers.Any(p => p.PlayerId == playerId)
                    || g.HomeBoxScore.Fielders.Any(f => f.PlayerId == playerId)
                ));
        }
    }
}
