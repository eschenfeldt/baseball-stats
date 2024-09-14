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

            // var statsQueryable = calculator.GetBattingStats();
            // var statsQuery = statsQueryable.ToQueryString();
            // var results = statsQueryable.ToList();

            var aggregateBatting = await calculator.GetBattingStats().FirstOrDefaultAsync();

            // var aggregateBatting = await _context.Constants
            //     .GroupJoin(_context.Batters.Where(b => b.PlayerId == id), c => c.Year, b => b.BoxScore.Game.Date.Year, (c, bg) => new
            //     {
            //         c.Year,
            //         Constants = c,
            //         Games = bg.Sum(b => b.Games),
            //         PlateAppearances = bg.Sum(b => b.PlateAppearances),
            //         AtBats = bg.Sum(b => b.AtBats),
            //         Hits = bg.Sum(b => b.Hits),
            //         Singles = bg.Sum(b => b.Singles),
            //         Doubles = bg.Sum(b => b.Doubles),
            //         Triples = bg.Sum(b => b.Triples),
            //         Homeruns = bg.Sum(b => b.Homeruns),
            //         Walks = bg.Sum(b => b.Walks),
            //         HitByPitch = bg.Sum(b => b.HitByPitch),
            //         SacrificeFlies = bg.Sum(b => b.SacrificeFlies),
            //     })
            //     .Select(y => new
            //     {
            //         y.Year,
            //         y.Games,
            //         y.PlateAppearances,
            //         WOBANum = y.Constants.WBB * y.Walks + y.Constants.W1B * y.Singles
            //             + y.Constants.W2B * y.Doubles + y.Constants.W3B * y.Triples
            //             + y.Constants.WHR * y.Homeruns + y.Constants.WHBP * y.HitByPitch,
            //         WOBADen = y.AtBats + y.Walks + y.SacrificeFlies + y.HitByPitch,
            //         OBPNum = y.Hits + y.Walks + y.HitByPitch,
            //         OBPDen = y.AtBats + y.Walks + y.HitByPitch + y.SacrificeFlies,
            //         y.Hits,
            //         y.AtBats
            //     })
            //     .GroupBy(y => true)
            //     .Select(yg => new
            //     {
            //         Games = yg.Sum(y => y.Games),
            //         PlateAppearances = yg.Sum(y => y.PlateAppearances),
            //         WOBA = yg.Sum(y => y.WOBADen) > 0 ? decimal.Divide(yg.Sum(y => y.WOBANum), yg.Sum(y => y.WOBADen)) : 0,
            //         OBP = yg.Sum(y => y.OBPDen) > 0 ? decimal.Divide(yg.Sum(y => y.OBPNum), yg.Sum(y => y.OBPDen)) : 0,
            //         AVG = yg.Sum(y => y.AtBats) > 0 ? decimal.Divide(yg.Sum(y => y.Hits), yg.Sum(y => y.AtBats)) : 0
            //     })
            //     .FirstOrDefaultAsync();

            if (aggregateBatting != null)
            {
                summary.SummaryStats.Add(new()
                {
                    Definition = Stat.Games,
                    Value = aggregateBatting.Games
                });
                summary.SummaryStats.Add(new()
                {
                    Definition = Stat.PlateAppearances,
                    Value = aggregateBatting.PlateAppearances
                });
                summary.SummaryStats.Add(new()
                {
                    Definition = Stat.OnBasePercentage,
                    Value = aggregateBatting.OBP
                });
                summary.SummaryStats.Add(new()
                {
                    Definition = Stat.BattingAverage,
                    Value = aggregateBatting.AVG
                });
                summary.SummaryStats.Add(new()
                {
                    Definition = Stat.WeightedOnBaseAverage,
                    Value = aggregateBatting.WOBA
                });
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
                Stats = StatCollection.GameStats
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
