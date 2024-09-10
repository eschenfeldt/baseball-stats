using BaseballApi.Models;
using BaseballApi.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BaseballApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LeaderboardController : ControllerBase
{
    private readonly BaseballContext _context;

    public LeaderboardController(BaseballContext context)
    {
        _context = context;
    }

    [HttpPost("batting")]
    public async Task<ActionResult<Leaderboard<LeaderboardPlayer>>> GetBattingLeaders(BatterLeaderboardParams leaderboardParams)
    {
        var calculator = new StatCalculator(_context)
        {
            Year = leaderboardParams.Year,
            PlayerSearch = leaderboardParams.PlayerSearch,
            MinPlateAppearances = leaderboardParams.MinPlateAppearances,
            OrderBy = leaderboardParams.Sort,
            OrderAscending = leaderboardParams.Asc
        };

        var stats = calculator.GetBattingStats();
        var query = stats
        .Skip(leaderboardParams.Skip)
        .Take(leaderboardParams.Take)
        .Select(s => new LeaderboardPlayer
        {
            // finally compute more complex stats
            Player = new(_context.Players.Single(p => s.PlayerId!.Value == p.Id)),
            Year = s.Year,
            Stats = s.ToDictionary()
        }).AsEnumerable();

        // sorting in postgres is working for skip/take but not final result order, so do that in-memory
        decimal? selector(LeaderboardPlayer b) => b.Stats[leaderboardParams.Sort];
        IOrderedEnumerable<LeaderboardPlayer> sorted = leaderboardParams.Asc ? query.OrderBy(selector) : query.OrderByDescending(selector);

        return new Leaderboard<LeaderboardPlayer>
        {
            Stats = StatCalculator.GetBattingStatDefs(),
            TotalCount = await stats.CountAsync(),
            Results = [.. sorted]
        };
    }

    [HttpPost("pitching")]
    public async Task<ActionResult<Leaderboard<LeaderboardPlayer>>> GetPitchingLeaders(PitcherLeaderboardParams leaderboardParams)
    {
        var calculator = new StatCalculator(_context)
        {
            Year = leaderboardParams.Year,
            PlayerSearch = leaderboardParams.PlayerSearch,
            MinInningsPitched = leaderboardParams.MinInningsPitched,
            OrderBy = leaderboardParams.Sort,
            OrderAscending = leaderboardParams.Asc
        };

        var stats = calculator.GetPitchingStats();
        var query = stats
        .Skip(leaderboardParams.Skip)
        .Take(leaderboardParams.Take)
        .Select(s => new LeaderboardPlayer
        {
            Player = new(_context.Players.Single(p => s.PlayerId!.Value == p.Id)),
            Year = s.Year,
            Stats = s.ToDictionary()
        }).AsEnumerable();

        // sorting in postgres is working for skip/take but not final result order, so do that in-memory
        decimal? selector(LeaderboardPlayer b) => b.Stats[leaderboardParams.Sort];
        IOrderedEnumerable<LeaderboardPlayer> sorted = leaderboardParams.Asc ? query.OrderBy(selector) : query.OrderByDescending(selector);

        return new Leaderboard<LeaderboardPlayer>
        {
            Stats = StatCalculator.GetPitchingStatDefs(),
            TotalCount = await stats.CountAsync(),
            Results = [.. sorted]
        };
    }
}
