using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaseballApi;
using BaseballApi.Models;
using BaseballApi.Contracts;

namespace BaseballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkController : ControllerBase
    {
        private readonly BaseballContext _context;

        public ParkController(BaseballContext context)
        {
            _context = context;
        }

        // GET: api/Park
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Park>>> GetParks()
        {
            return await _context.Parks.ToListAsync();
        }

        // GET: api/Park/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Park>> GetPark(long id)
        {
            var park = await _context.Parks.FindAsync(id);

            if (park == null)
            {
                return NotFound();
            }

            return park;
        }

        [HttpGet("summaries")]
        public async Task<ActionResult<PagedResult<ParkSummary>>> GetParkSummaries(int skip, int take, string? sort = null, bool asc = false, long? teamId = null)
        {
            ParkSummaryOrder order = sort.ToEnumOrDefault<ParkSummaryOrder, ParamValueAttribute>();

            IQueryable<Game> games = _context.Games;
            if (teamId.HasValue)
            {
                games = games.Where(g => g.Away.Id == teamId || g.Home.Id == teamId);
            }

            IQueryable<ParkSummary> parks = _context.Parks
                .GroupJoin(games, p => p.Id, g => g.LocationId, (park, games) => new ParkSummary
                {
                    Park = park,
                    Games = games.Count(),
                    Teams = games.Select(g => g.Away.Id).Union(games.Select(g => g.Home.Id)).Distinct().Count(),
                    Wins = games.Count(g => g.WinningTeam != null && g.WinningTeam == g.Home),
                    Losses = games.Count(g => g.LosingTeam != null && g.LosingTeam == g.Home),
                    FirstGameDate = games.Select(g => g.Date).DefaultIfEmpty().Min(),
                    LastGameDate = games.Select(g => g.Date).DefaultIfEmpty().Max()
                });

            var sorted = GetSorted(parks, order, asc);
            return new PagedResult<ParkSummary>
            {
                TotalCount = parks.Count(),
                Results = await sorted
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync()
            };
        }

        private static IOrderedQueryable<ParkSummary> GetSorted(IQueryable<ParkSummary> query, ParkSummaryOrder order, bool asc)
        {
            if (asc)
            {
                return order switch
                {
                    ParkSummaryOrder.Teams => query.OrderBy(k => k.Teams),
                    ParkSummaryOrder.Games => query.OrderBy(k => k.Games),
                    ParkSummaryOrder.Wins => query.OrderBy(k => k.Wins),
                    ParkSummaryOrder.Losses => query.OrderBy(k => k.Losses),
                    ParkSummaryOrder.LastGame => query.OrderBy(k => k.LastGameDate),
                    ParkSummaryOrder.Park => query.OrderBy(k => k.Park.Name),
                    _ => query.OrderBy(k => k.Games),
                };
            }
            else
            {
                return order switch
                {
                    ParkSummaryOrder.Teams => query.OrderByDescending(k => k.Teams),
                    ParkSummaryOrder.Games => query.OrderByDescending(k => k.Games),
                    ParkSummaryOrder.Wins => query.OrderByDescending(k => k.Wins),
                    ParkSummaryOrder.Losses => query.OrderByDescending(k => k.Losses),
                    ParkSummaryOrder.LastGame => query.OrderByDescending(k => k.LastGameDate),
                    ParkSummaryOrder.Park => query.OrderByDescending(k => k.Park.Name),
                    _ => query.OrderByDescending(k => k.Games),
                };
            }
        }
    }
}
