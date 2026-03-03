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
using Humanizer;
using BaseballApi.Import;

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
        public async Task<ActionResult<PlayerSummary>> GetPlayer(long id, long? teamId = null, long? parkId = null, int? year = null, GameType? gameType = null)
        {
            Player? player = await _context.Players
                .Include(p => p.Media)
                    .ThenInclude(m => m.Files)
                .Where(p => p.Id == id)
                .SingleOrDefaultAsync();

            if (player == null)
            {
                return NotFound();
            }

            return await this.GetPlayerSummary(player, teamId, parkId, year, gameType);
        }

        [HttpGet("random")]
        public async Task<ActionResult<PlayerSummary>> GetRandomPlayer(bool withMedia = false)
        {
            var playerId = await _context.Database.SqlQuery<long?>($@"
                SELECT p.""Id"" AS ""Value""
                FROM ""Players"" p
                LEFT JOIN ""Batters"" b ON b.""PlayerId"" = p.""Id""
                LEFT JOIN ""Pitchers"" pi ON pi.""PlayerId"" = p.""Id""
                LEFT JOIN ""Fielders"" f ON f.""PlayerId"" = p.""Id""
                WHERE b.""Id"" IS NOT NULL
                    OR pi.""Id"" IS NOT NULL
                    OR f.""Id"" IS NOT NULL
                GROUP BY p.""Id""
                ORDER BY RANDOM() LIMIT 1")
                .SingleOrDefaultAsync();

            Player? player = await _context.Players
                .Include(p => p.Media)
                    .ThenInclude(m => m.Files)
                .Where(p => p.Id == playerId)
                .SingleOrDefaultAsync();

            if (player == null)
            {
                return NotFound();
            }

            return await this.GetPlayerSummary(player);
        }

        private async Task<PlayerSummary> GetPlayerSummary(Player player, long? teamId = null, long? parkId = null, int? year = null, GameType? gameType = null)
        {
            PlayerSummary summary = new()
            {
                Info = new(player),
                SummaryStats = []
            };

            var calculator = new StatCalculator(_context)
            {
                PlayerId = player.Id,
                TeamId = teamId,
                ParkId = parkId,
                GameType = gameType,
                Year = year
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

            var size = "large";
            summary.Photo = player.Media.Select(r => r.Files.First(f =>
                    f.Purpose == RemoteFilePurpose.Thumbnail
                    && f.NameModifier != null
                    && f.NameModifier == size))
                .Select(f => new RemoteFileDetail
                {
                    AssetIdentifier = f.Resource.AssetIdentifier,
                    DateTime = f.Resource.DateTime,
                    FileType = (f.Resource as MediaResource).ResourceType.Humanize(),
                    OriginalFileName = f.Resource.OriginalFileName,
                    NameModifier = f.NameModifier,
                    Purpose = f.Purpose,
                    Extension = f.Extension
                }).OrderBy(f => Guid.NewGuid()).Cast<RemoteFileDetail?>().FirstOrDefault();

            return summary;
        }

        [HttpGet("games")]
        public async Task<ActionResult<PlayerGameResults>> GetGames(
            long playerId,
            int skip = 0,
            int take = 10,
            bool asc = false,
            int? year = null,
            long? teamId = null,
            long? parkId = null,
            GameType? gameType = null)
        {
            IQueryable<Game> games = _context.Games;
            if (year.HasValue)
            {
                games = games.Where(g => g.Date.Year == year);
            }
            if (parkId.HasValue)
            {
                games = games.Where(g => g.LocationId == parkId);
            }
            if (gameType.HasValue)
            {
                games = games.Where(g => g.GameType == gameType);
            }

            var query = PlayerManager.ConstructPlayerGamesQuery(playerId, games, teamId)
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
    }
}
