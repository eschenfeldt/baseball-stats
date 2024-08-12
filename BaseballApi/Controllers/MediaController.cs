using BaseballApi.Contracts;
using BaseballApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly BaseballContext _context;

        public MediaController(BaseballContext context)
        {
            _context = context;
        }

        [HttpGet("thumbnails")]
        public Task<ActionResult<PagedResult<RemoteFileDetail>>> GetThumbnails(
            int skip = 0,
            int take = 50,
            bool asc = false,
            long? gameId = null,
            long? playerId = null
        )
        {
            var query = _context.MediaResources;

            throw new NotImplementedException();
        }
    }
}
