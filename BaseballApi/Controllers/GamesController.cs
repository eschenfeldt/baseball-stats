using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaseballApi.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using BaseballApi.Contracts;

namespace BaseballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly BaseballContext _context;

        public GamesController(BaseballContext context)
        {
            _context = context;
        }

        // GET: api/Games
        [HttpGet]
        public async Task<ActionResult<PagedResult<GameSummary>>> GetGames(
            int skip = 0,
            int take = 10,
            bool asc = false,
            long? teamId = null,
            int? year = null)
        {
            var query = _context.Games
                .Include(nameof(Game.Away))
                .Include(nameof(Game.Home))
                .Include(nameof(Game.Location))
                .Include(nameof(Game.WinningTeam))
                .Include(nameof(Game.LosingTeam))
                .Include(nameof(Game.WinningPitcher))
                .Include(nameof(Game.LosingPitcher))
                .Include(nameof(Game.LosingTeam));

            if (teamId.HasValue)
            {
                query = query.Where(g => g.Away.Id == teamId || g.Home.Id == teamId);
            }
            if (year.HasValue)
            {
                query = query.Where(g => g.Date.Year == year);
            }

            var sorted = asc ? query.OrderBy(g => g.StartTime ?? g.ScheduledTime ?? g.Date.ToDateTime(TimeOnly.MinValue))
                : query.OrderByDescending(g => g.StartTime ?? g.ScheduledTime ?? g.Date.ToDateTime(TimeOnly.MinValue));

            return new PagedResult<GameSummary>
            {
                TotalCount = await query.CountAsync(),
                Results = await sorted
                    .Select(g => new GameSummary(g))
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync()
            };
        }

        // GET: api/Games/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GameDetail>> GetGame(long id)
        {
            var game = await _context.Games
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
                .AsSplitQuery()
                .SingleOrDefaultAsync(g => g.Id == id);

            if (game == null)
            {
                return NotFound();
            }

            return new GameDetail(game);
        }

        // [HttpGet("{gameId}/boxscore")]
        // public async Task<ActionResult<BoxScoreDetail>> GetBoxScore(long gameId, bool home = true)
        // {
        //     var box = await _context.Games
        //         .Where(g => g.Id == gameId)
        //         .Select(g => home ? g.HomeBoxScore : g.AwayBoxScore)
        //         .Include(b => b.Batters)
        //             .ThenInclude(b => b.Player)
        //         .Include(b => b.Pitchers)
        //             .ThenInclude(p => p.Player)
        //         .Include(b => b.Fielders)
        //             .ThenInclude(f => f.Player)
        //         .AsSplitQuery()
        //         .SingleOrDefaultAsync();

        //     if (box == null)
        //     {
        //         return NotFound();
        //     }

        //     return new BoxScoreDetail(box);
        // }

        [HttpGet("years")]
        public async Task<ActionResult<List<int>>> GetGameYears(long? teamId = null)
        {
            IQueryable<Game> query = _context.Games;

            if (teamId.HasValue)
            {
                query = query.Where(g => g.Away.Id == teamId || g.Home.Id == teamId);
            }
            return await query.Select(g => g.Date.Year).Distinct().OrderBy(i => i).ToListAsync();
        }

        [HttpPost("import")]
        [Authorize]
        public async Task<IActionResult> ImportGame([FromForm] List<IFormFile> files, [FromForm] string serializedMetadata)
        {
            GameMetadata metadata = JsonConvert.DeserializeObject<GameMetadata>(serializedMetadata);
            long size = files.Sum(f => f.Length);

            var localFilePaths = new Dictionary<string, string>();
            foreach (var formFile in files)
            {
                var filePath = Path.GetTempFileName();
                Console.WriteLine(filePath);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await formFile.CopyToAsync(stream);
                }
                localFilePaths[formFile.FileName] = filePath;
            }

            GameImportManager importManager = new(new GameImportData
            {
                FilePaths = localFilePaths,
                Metadata = metadata
            }, _context);

            Game newGame = await importManager.GetGame();
            BoxScore homeBox = new()
            {
                Game = newGame,
                Team = newGame.Home
            };
            importManager.PopulateBoxScore(homeBox, home: true);
            int homeScoreBatters = homeBox.Batters.Select(b => b.Runs).Sum();
            newGame.HomeScore = homeScoreBatters;

            BoxScore awayBox = new()
            {
                Game = newGame,
                Team = newGame.Away
            };
            importManager.PopulateBoxScore(awayBox, home: false);
            int awayScoreBatters = awayBox.Batters.Select(b => b.Runs).Sum();
            newGame.AwayScore = awayScoreBatters;

            bool isNew = false;

            // TODO: make this check more robust
            Game? existingGame = await _context.Games.FirstOrDefaultAsync(g => g.Name == newGame.Name);
            if (existingGame != null)
            {
                existingGame.Name = newGame.Name;
                existingGame.Date = newGame.Date;
                existingGame.GameType = newGame.GameType;
                existingGame.Home = newGame.Home;
                existingGame.HomeTeamName = newGame.HomeTeamName;
                existingGame.Away = newGame.Away;
                existingGame.AwayTeamName = newGame.AwayTeamName;
                existingGame.ScheduledTime = newGame.ScheduledTime;
                existingGame.StartTime = newGame.StartTime;
                existingGame.EndTime = newGame.EndTime;
                existingGame.Location = newGame.Location;
                existingGame.HomeScore = newGame.HomeScore;
                existingGame.AwayScore = newGame.AwayScore;
                existingGame.WinningTeam = newGame.WinningTeam;
                existingGame.LosingTeam = newGame.LosingTeam;
                existingGame.WinningPitcher = newGame.WinningPitcher;
                existingGame.LosingPitcher = newGame.LosingPitcher;
                existingGame.SavingPitcher = newGame.SavingPitcher;
                // update box scores

            }
            else
            {
                await _context.Games.AddAsync(newGame);
                isNew = true;
            }

            var changes = await _context.SaveChangesAsync();

            if (isNew)
            {
                newGame.HomeBoxScore = homeBox;
                newGame.AwayBoxScore = awayBox;

                await _context.SaveChangesAsync();
            }

            return Ok(new { count = files.Count, size, metadata, changes });
        }

        // DELETE: api/Games/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteGame(long id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GameExists(long id)
        {
            return _context.Games.Any(e => e.Id == id);
        }
    }
}
