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
using Microsoft.AspNetCore.Http.HttpResults;

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
            long? parkId = null,
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
            if (parkId.HasValue)
            {
                query = query.Where(g => g.LocationId == parkId);
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

        [HttpGet("on-date")]
        public async Task<ActionResult<List<GameSummary>>> GetGamesOnDate(int month, int day)
        {
            IQueryable<Game> query = _context.Games
                .Where(g => g.Date.Month == month && g.Date.Day == day)
                .Include(nameof(Game.Away))
                .Include(nameof(Game.Home))
                .Include(nameof(Game.Location))
                .Include(nameof(Game.WinningTeam))
                .Include(nameof(Game.LosingTeam))
                .Include(nameof(Game.WinningPitcher))
                .Include(nameof(Game.LosingPitcher))
                .Include(nameof(Game.LosingTeam))
                .Include(g => g.Media);

            return await query.OrderBy(g => g.Date).ThenBy(g => g.StartTime)
                            .Select(g => new GameSummary(g))
                            .ToListAsync();
        }

        [HttpGet("years")]
        public async Task<ActionResult<List<int>>> GetGameYears(long? teamId = null, long? parkId = null)
        {
            IQueryable<Game> query = _context.Games;

            if (teamId.HasValue)
            {
                query = query.Where(g => g.Away.Id == teamId || g.Home.Id == teamId);
            }
            if (parkId.HasValue)
            {
                query = query.Where(g => g.LocationId == parkId);
            }
            return await query.Select(g => g.Date.Year).Distinct().OrderBy(i => i).ToListAsync();
        }

        [HttpGet("summary-stats")]
        public async Task<ActionResult<List<SummaryStat>>> GetSummaryStats(long? teamId = null, long? parkId = null)
        {
            IQueryable<Game> gamesQuery = _context.Games;

            IQueryable<BoxScore?> nullableBoxScoresQuery;
            int? winCount = null;
            int? lossCount = null;
            if (teamId.HasValue && parkId.HasValue)
            {
                throw new NotImplementedException("Filtering by both team and park is not implemented yet.");
            }
            else if (teamId.HasValue)
            {
                nullableBoxScoresQuery = gamesQuery.Where(g => g.Away.Id == teamId).Select(g => g.AwayBoxScore)
                    .Concat(gamesQuery.Where(g => g.Home.Id == teamId).Select(g => g.HomeBoxScore));
                gamesQuery = gamesQuery.Where(g => g.Away.Id == teamId || g.Home.Id == teamId);
                winCount = await gamesQuery.CountAsync(g => g.WinningTeam != null && g.WinningTeam.Id == teamId);
                lossCount = await gamesQuery.CountAsync(g => g.LosingTeam != null && g.LosingTeam.Id == teamId);
            }
            else if (parkId.HasValue)
            {
                gamesQuery = gamesQuery.Where(g => g.LocationId == parkId);
                nullableBoxScoresQuery = gamesQuery.Select(g => g.HomeBoxScore).Concat(gamesQuery.Select(g => g.AwayBoxScore));
                winCount = await gamesQuery.CountAsync(g => g.WinningTeam != null && g.WinningTeam == g.Home);
                lossCount = await gamesQuery.CountAsync(g => g.LosingTeam != null && g.LosingTeam == g.Home);
            }
            else
            {
                nullableBoxScoresQuery = gamesQuery.Select(g => g.HomeBoxScore).Concat(gamesQuery.Select(g => g.AwayBoxScore));
            }
            IQueryable<BoxScore> boxScoresQuery = nullableBoxScoresQuery.Where(bs => bs != null).Select(bs => bs!);

            var gameCount = await gamesQuery.CountAsync();
            var parksCount = await gamesQuery.Where(g => g.Location != null)
                .Select(g => g.Location).Distinct().CountAsync();
            int teamsCount = await gamesQuery.Select(g => g.Away.Id).Union(gamesQuery.Select(g => g.Home.Id)).CountAsync();
            int playerCount = await boxScoresQuery.SelectMany(bs => bs.Batters.Select(b => b.PlayerId))
                                .Union(boxScoresQuery.SelectMany(bs => bs.Pitchers.Select(p => p.PlayerId)))
                                .Union(boxScoresQuery.SelectMany(bs => bs.Fielders.Select(f => f.PlayerId)))
                                .CountAsync();

            var result = new List<SummaryStat>
            {
                new()
                {
                    Category = StatCategory.General,
                    Definition = Stat.Games,
                    Value = gameCount
                },
                new()
                {
                    Category = StatCategory.General,
                    Definition = Stat.Wins,
                    Value = winCount
                },
                new()
                {
                    Category = StatCategory.General,
                    Definition = Stat.Losses,
                    Value = lossCount
                },
                new()
                {
                    Category = StatCategory.General,
                    Definition = Stat.Parks,
                    Value = parkId.HasValue ? null : parksCount // Hide rather than always returning 1
                },
                new()
                {
                    Category = StatCategory.General,
                    Definition = Stat.Teams,
                    Value = teamsCount
                },
                new()
                {
                    Category = StatCategory.General,
                    Definition = Stat.Players,
                    Value = playerCount
                }
            };

            var calculator = new StatCalculator(_context)
            {
                TeamId = teamId,
                ParkId = parkId,
                GroupByPlayer = false
            };

            var aggregateBatting = await calculator.GetBattingStats().FirstOrDefaultAsync();
            aggregateBatting?.AddAsSummaryStats(result);
            var aggregatePitching = await calculator.GetPitchingStats().FirstOrDefaultAsync();
            aggregatePitching?.AddAsSummaryStats(result);

            return result;
        }

        [HttpPost("import")]
        [Authorize]
        public async Task<ActionResult<GameImportResult>> ImportGame([FromForm] List<IFormFile> files, [FromForm] string serializedMetadata)
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
            importManager.AddLocation(newGame);
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

            return new GameImportResult
            {
                Id = newGame.Id,
                Count = files.Count,
                Size = size,
                Metadata = metadata,
                Changes = changes
            };
        }
    }
}
