using BaseballApi.Models;
using BaseballApi.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<PagedResult<LeaderboardBatter>>> GetBattingLeaders(BatterLeaderboardParams leaderboardParams)
    {
        IQueryable<Batter> batters = _context.Batters;
        if (leaderboardParams.Year.HasValue)
        {
            batters = batters.Where(b => b.BoxScore.Game.Date.Year == leaderboardParams.Year);
        }
        var query = _context.Players.GroupJoin(
            batters,
            p => p.Id,
            b => b.PlayerId,
            (p, b) => new { Player = p, Batters = b }
        )
        .Where(p => p.Batters.Any())
        .Select(p => new
        {
            // now compute sums
            p.Player,
            Games = p.Batters.Select(b => b.Games).Sum(),
            Hits = p.Batters.Select(b => b.Hits).Sum(),
            AtBats = p.Batters.Select(b => b.AtBats).Sum(),
            PlateAppearances = p.Batters.Select(b => b.PlateAppearances).Sum()
        })
        .Where(p => p.PlateAppearances > leaderboardParams.MinPlateAppearances)
        .Select(p => new LeaderboardBatter
        {
            // finally compute more complex stats
            Player = p.Player,
            Year = leaderboardParams.Year,
            Games = p.Games,
            AtBats = p.AtBats,
            Hits = p.Hits,
            BattingAverage = p.AtBats > 0 ? decimal.Divide(p.Hits, p.AtBats) : null
        });

        var sorted = GetSorted(query, leaderboardParams.Order, leaderboardParams.SortAscending);

        return new PagedResult<LeaderboardBatter>
        {
            TotalCount = await query.CountAsync(),
            Results = await sorted.Skip(leaderboardParams.Skip)
                            .Take(leaderboardParams.Take)
                            .ToListAsync()
        };

    }

    private static IOrderedQueryable<LeaderboardBatter> GetSorted(IQueryable<LeaderboardBatter> query, BatterLeaderboardOrder order, bool asc)
    {
        if (asc)
        {
            return order switch
            {
                BatterLeaderboardOrder.Games => query.OrderBy(k => k.Games),
                BatterLeaderboardOrder.BattingAverage => query.OrderBy(k => k.BattingAverage),
                _ => query.OrderBy(k => k.Games),
            };
        }
        else
        {
            return order switch
            {
                BatterLeaderboardOrder.Games => query.OrderByDescending(k => k.Games),
                BatterLeaderboardOrder.BattingAverage => query.OrderByDescending(k => k.BattingAverage),
                _ => query.OrderByDescending(k => k.Games),
            };
        }
    }
}
