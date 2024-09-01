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
    public async Task<ActionResult<Leaderboard<LeaderboardBatter>>> GetBattingLeaders(BatterLeaderboardParams leaderboardParams)
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
        .Select(s => new LeaderboardBatter
        {
            // finally compute more complex stats
            Player = new(_context.Players.Single(p => s.PlayerId!.Value == p.Id)),
            Year = s.Year,
            Stats = new Dictionary<string, decimal?>{
                {Stat.Games.Name, s.Games},
                {Stat.PlateAppearances.Name, s.PlateAppearances},
                {Stat.BattingAverage.Name, s.AVG},
                {Stat.OnBasePercentage.Name, s.OBP},
                {Stat.WeightedOnBaseAverage.Name, s.WOBA}
            }
        }).AsEnumerable();

        // sorting in postgres is working for skip/take but not final result order, so do that in-memory
        Func<LeaderboardBatter, decimal?> selector = b => b.Stats[leaderboardParams.Sort];
        IOrderedEnumerable<LeaderboardBatter> sorted = leaderboardParams.Asc ? query.OrderBy(selector) : query.OrderByDescending(selector);

        return new Leaderboard<LeaderboardBatter>
        {
            Stats = StatCalculator.GetBattingStats(),
            TotalCount = await stats.CountAsync(),
            Results = sorted.ToList()
        };

    }
}
