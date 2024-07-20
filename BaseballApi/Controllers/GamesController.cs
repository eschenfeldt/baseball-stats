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
        public async Task<ActionResult<PagedResult<GameSummary>>> GetGames(int skip = 0, int take = 10)
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

            return new PagedResult<GameSummary>
            {
                TotalCount = await query.CountAsync(),
                Results = await query
                    .OrderBy(g => g.Date)
                    .Select(g => new GameSummary(g))
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync()
            };
        }

        // GET: api/Games/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Game>> GetGame(long id)
        {
            var game = await _context.Games.FindAsync(id);

            if (game == null)
            {
                return NotFound();
            }

            return game;
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
                localFilePaths[formFile.Name] = filePath;
            }

            GameImportManager importManager = new(new GameImportData
            {
                FilePaths = localFilePaths,
                Metadata = metadata
            });

            Game newGame = importManager.GetGame();

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
            }
            else
            {
                await _context.Games.AddAsync(newGame);
            }

            var changes = await _context.SaveChangesAsync();

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
