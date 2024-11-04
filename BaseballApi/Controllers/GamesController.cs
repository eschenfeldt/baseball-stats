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
using BaseballApi.Import;

namespace BaseballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly BaseballContext _context;
        IRemoteFileManager RemoteFileManager { get; }

        public GamesController(BaseballContext context, IRemoteFileManager remoteFileManager)
        {
            _context = context;
            RemoteFileManager = remoteFileManager;
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
                .Include(g => g.Location)
                .Include(g => g.Home)
                .Include(g => g.Away)
                .Include(g => g.Scorecard)
                    .ThenInclude(s => s.Files)
                .Include(g => g.Media)
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

        [HttpGet("random")]
        public async Task<ActionResult<GameSummary>> GetRandomGame(bool withMedia = false)
        {
            IQueryable<Game> query = _context.Games
                .Include(nameof(Game.Away))
                .Include(nameof(Game.Home))
                .Include(nameof(Game.Location))
                .Include(nameof(Game.WinningTeam))
                .Include(nameof(Game.LosingTeam))
                .Include(nameof(Game.WinningPitcher))
                .Include(nameof(Game.LosingPitcher))
                .Include(nameof(Game.LosingTeam))
                .Include(g => g.Media);

            if (withMedia)
            {
                query = query.Where(g => g.Media.Count > 0);
            }

            return await query
                .OrderBy(g => Guid.NewGuid())
                .Select(g => new GameSummary(g))
                .FirstOrDefaultAsync();
        }

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
                // TODO: update box scores
                if (existingGame.Scorecard != null)
                {
                    await this.RemoteFileManager.DeleteResource(existingGame.Scorecard);
                    _context.Scorecards.Remove(existingGame.Scorecard);
                }
                existingGame.Scorecard = newGame.Scorecard;
            }
            else
            {
                await _context.Games.AddAsync(newGame);
                isNew = true;
            }

            if (newGame.Scorecard != null && newGame.Scorecard.Files.Count == 1 && importManager.ScorecardFilePath != null)
            {
                await this.RemoteFileManager.UploadFile(newGame.Scorecard.Files.Single(), importManager.ScorecardFilePath);
            }

            var changes = await _context.SaveChangesAsync();

            if (isNew)
            {
                newGame.HomeBoxScore = homeBox;
                newGame.AwayBoxScore = awayBox;

                await _context.SaveChangesAsync();
            }

            return Ok(new { id = newGame.Id, count = files.Count, size, metadata, changes });
        }
    }
}
