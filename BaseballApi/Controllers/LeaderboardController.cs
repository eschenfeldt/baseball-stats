using BaseballApi.Models;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("batting")]
    public async Task<ActionResult<IEnumerable<LeaderboardBatter>>> GetBattingLeaders(LeaderboardParams leaderboardParams)
    {
        throw new NotImplementedException();
    }
}
