using BaseballApi.Contracts;
using BaseballApi.Models;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
        public async Task<ActionResult<PagedResult<RemoteFileDetail>>> GetThumbnails(
            int skip = 0,
            int take = 50,
            bool asc = false,
            string size = "small",
            long? gameId = null,
            long? playerId = null
        )
        {
            IQueryable<MediaResource> query = _context.MediaResources;

            if (gameId.HasValue)
            {
                query = query.Where(r => r.Game != null && r.Game.Id == gameId);
            }
            if (playerId.HasValue)
            {
                query = query.Where(r => r.Players.Any(p => p.Id == playerId));
            }

            IOrderedQueryable<MediaResource> sortedResources;
            if (asc)
            {
                sortedResources = query.OrderBy(r => r.DateTime);
            }
            else
            {
                sortedResources = query.OrderByDescending(r => r.DateTime);
            }

            var allResults = sortedResources
                .Select(r => r.Files.First(f =>
                    f.Purpose == RemoteFilePurpose.Thumbnail
                    && f.NameModifier != null
                    && f.NameModifier == size))
                .Select(f => new RemoteFileDetail
                {
                    AssetIdentifier = f.Resource.AssetIdentifier,
                    DateTime = f.Resource.DateTime,
                    FileType = (f.Resource as MediaResource).ResourceType.Humanize(),
                    OriginalFileName = f.Resource.OriginalFileName,
                    NameModifier = f.NameModifier,
                    Purpose = f.Purpose,
                    Extension = f.Extension
                });

            return new PagedResult<RemoteFileDetail>
            {
                TotalCount = await allResults.CountAsync(),
                Results = await allResults.Skip(skip).Take(take).ToListAsync()
            };
        }
    }
}
