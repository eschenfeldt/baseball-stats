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

        private static readonly HashSet<string> VIDEO_EXTENSIONS =
        [
            ".mov",
            ".MOV"
        ];

        [HttpGet("original/{assetIdentifier}")]
        public async Task<ActionResult<RemoteOriginal>> GetOriginal(Guid assetIdentifier)
        {
            return await _context.MediaResources
                    .Where(r => r.AssetIdentifier == assetIdentifier)
                    .Select(r => new RemoteOriginal
                    {
                        FileType = r.ResourceType.Humanize(),
                        GameName = r.Game != null ? r.Game.Name : null,
                        Photo = r.ResourceType == MediaResourceType.Photo || r.ResourceType == MediaResourceType.LivePhoto ? r.Files.Where(f => f.Purpose == RemoteFilePurpose.Original && !VIDEO_EXTENSIONS.Contains(f.Extension))
                        .Select(f => new RemoteFileDetail
                        {
                            AssetIdentifier = r.AssetIdentifier,
                            DateTime = r.DateTime,
                            FileType = r.ResourceType.Humanize(),
                            OriginalFileName = r.OriginalFileName,
                            NameModifier = f.NameModifier,
                            Purpose = f.Purpose,
                            Extension = f.Extension
                        }).SingleOrDefault() : null,
                        Video = r.ResourceType == MediaResourceType.Video || r.ResourceType == MediaResourceType.LivePhoto ? r.Files.Where(f => f.Purpose == RemoteFilePurpose.Original && VIDEO_EXTENSIONS.Contains(f.Extension))
                        .Select(f => new RemoteFileDetail
                        {
                            AssetIdentifier = r.AssetIdentifier,
                            DateTime = r.DateTime,
                            FileType = r.ResourceType.Humanize(),
                            OriginalFileName = r.OriginalFileName,
                            NameModifier = f.NameModifier,
                            Purpose = f.Purpose,
                            Extension = f.Extension
                        }).SingleOrDefault() : null
                    }).SingleOrDefaultAsync();
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
