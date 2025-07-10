using System.Threading.Tasks;
using BaseballApi.Contracts;
using BaseballApi.Import;
using BaseballApi.Media;
using BaseballApi.Models;
using BaseballApi.Services;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BaseballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController(BaseballContext context, IRemoteFileManager remoteFileManager, IMediaImportQueue mediaImportQueue) : ControllerBase
    {
        private readonly BaseballContext _context = context;
        IRemoteFileManager RemoteFileManager { get; } = remoteFileManager;
        IMediaImportQueue MediaImportQueue { get; } = mediaImportQueue;

        private static readonly HashSet<string> VIDEO_EXTENSIONS =
        [
            ".mov",
            ".MOV",
            ".mp4"
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
                        AlternatePhoto = r.ResourceType == MediaResourceType.Photo || r.ResourceType == MediaResourceType.LivePhoto ? r.Files.Where(f => f.Purpose == RemoteFilePurpose.AlternateFormat && !VIDEO_EXTENSIONS.Contains(f.Extension))
                        .Select(f => new RemoteFileDetail
                        {
                            AssetIdentifier = r.AssetIdentifier,
                            DateTime = r.DateTime,
                            FileType = r.ResourceType.Humanize(),
                            OriginalFileName = r.OriginalFileName,
                            NameModifier = f.NameModifier,
                            Purpose = f.Purpose,
                            Extension = f.Extension
                        } as RemoteFileDetail?).FirstOrDefault() : null,
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
                        }).SingleOrDefault() : null,
                        AlternateVideo = r.ResourceType == MediaResourceType.Video || r.ResourceType == MediaResourceType.LivePhoto ? r.Files.Where(f => f.Purpose == RemoteFilePurpose.AlternateFormat && VIDEO_EXTENSIONS.Contains(f.Extension))
                        .Select(f => new RemoteFileDetail
                        {
                            AssetIdentifier = r.AssetIdentifier,
                            DateTime = r.DateTime,
                            FileType = r.ResourceType.Humanize(),
                            OriginalFileName = r.OriginalFileName,
                            NameModifier = f.NameModifier,
                            Purpose = f.Purpose,
                            Extension = f.Extension
                        } as RemoteFileDetail?).FirstOrDefault() : null
                    }).SingleOrDefaultAsync();
        }

        [HttpGet("thumbnails")]
        public async Task<ActionResult<PagedResult<RemoteFileDetail>>> GetThumbnails(
            int skip = 0,
            int take = 50,
            bool asc = true,
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

        [HttpGet("thumbnail/{assetIdentifier}")]
        public async Task<ActionResult<RemoteFileDetail?>> GetThumbnail(Guid assetIdentifier, string size = "small")
        {
            return await _context.MediaResources
                .Where(r => r.AssetIdentifier == assetIdentifier)
                .Select(r => r.Files.First(f =>
                    f.Purpose == RemoteFilePurpose.Thumbnail && f.NameModifier != null && f.NameModifier == size))
                .Select(f => new RemoteFileDetail
                {
                    AssetIdentifier = f.Resource.AssetIdentifier,
                    DateTime = f.Resource.DateTime,
                    FileType = (f.Resource as MediaResource).ResourceType.Humanize(),
                    OriginalFileName = f.Resource.OriginalFileName,
                    NameModifier = f.NameModifier,
                    Purpose = f.Purpose,
                    Extension = f.Extension
                }).SingleOrDefaultAsync();
        }

        [HttpGet("random")]
        public async Task<ActionResult<RemoteFileDetail?>> GetRandomThumbnail(
            string size = "medium",
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

            var allResults = query.OrderBy(r => Guid.NewGuid())
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

            return await allResults.Cast<RemoteFileDetail?>().FirstOrDefaultAsync();
        }

        [HttpPost("import-scorecard")]
        [Authorize]
        public async Task<IActionResult> ImportScorecard([FromForm] IFormFile file, [FromForm] string serializedGameId)
        {
            long gameId = JsonConvert.DeserializeObject<long>(serializedGameId);

            var filePath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            Game? game = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null)
            {
                return NotFound();
            }

            Scorecard scorecard = new()
            {
                AssetIdentifier = Guid.NewGuid(),
                OriginalFileName = file.FileName
            };
            if (game.EndTime.HasValue)
            {
                scorecard.DateTime = game.EndTime.Value.ToUniversalTime();
            }
            var extension = Path.GetExtension(scorecard.OriginalFileName);
            var remoteFile = new RemoteFile
            {
                Resource = scorecard,
                Purpose = RemoteFilePurpose.Original,
                Extension = extension
            };
            scorecard.Files.Add(remoteFile);
            await RemoteFileManager.UploadFile(remoteFile, filePath);

            game.Scorecard = scorecard;
            await _context.SaveChangesAsync();

            ScorecardDetail toReturn = new(scorecard);

            return Ok(new { scorecard = toReturn });
        }

        [HttpPost("import-media")]
        [Authorize]
        public async Task<ActionResult<ImportTask>> ImportMedia([FromForm] List<IFormFile> files, [FromForm] string serializedGameId)
        {
            long gameId = JsonConvert.DeserializeObject<long>(serializedGameId);
            Game? game = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);
            if (game == null)
            {
                return NotFound();
            }

            var resources = new Dictionary<string, MediaImportInfo>();
            foreach (var formFile in files)
            {
                var filePath = Path.GetTempFileName();
                filePath = Path.ChangeExtension(filePath, Path.GetExtension(formFile.FileName));
                using (var stream = System.IO.File.Create(filePath))
                {
                    await formFile.CopyToAsync(stream);
                }
                var baseName = Path.GetFileNameWithoutExtension(formFile.FileName);
                if (resources.TryGetValue(baseName, out MediaImportInfo? resource))
                {
                    if (resource.ResourceType == MediaResourceType.Photo && formFile.ContentType.StartsWith("video/"))
                    {
                        resource.VideoFileName = formFile.FileName;
                        resource.VideoFilePath = filePath;
                        resource.ResourceType = MediaResourceType.LivePhoto;
                    }
                    else if (resource.ResourceType == MediaResourceType.Video && formFile.ContentType.StartsWith("image/"))
                    {
                        resource.PhotoFileName = formFile.FileName;
                        resource.PhotoFilePath = filePath;
                        resource.ResourceType = MediaResourceType.LivePhoto;
                    }
                    else
                    {
                        throw new ArgumentException($"Duplicate file name '{baseName}' with unexpected types: {resource.ResourceType} and {formFile.ContentType}");
                    }
                }
                else if (formFile.ContentType.StartsWith("image/"))
                {
                    resource = new MediaImportInfo
                    {
                        BaseName = baseName,
                        ResourceType = MediaResourceType.Photo,
                        PhotoFileName = formFile.FileName,
                        PhotoFilePath = filePath
                    };
                    resources[baseName] = resource;
                }
                else if (formFile.ContentType.StartsWith("video/"))
                {
                    resource = new MediaImportInfo
                    {
                        BaseName = baseName,
                        ResourceType = MediaResourceType.Video,
                        VideoFileName = formFile.FileName,
                        VideoFilePath = filePath
                    };
                    resources[baseName] = resource;
                }
                else
                {
                    throw new ArgumentException($"Unsupported file type '{formFile.ContentType}' for file '{formFile.FileName}'");
                }
            }

            var task = await this.SaveImportTask([.. resources.Select(kvp => kvp.Value)], game);
            // queue the import task for processing
            await MediaImportQueue.PushAsync(task.Id);
            return task;
        }

        [HttpPost("restart-import-task/{taskId}")]
        [Authorize]
        public async Task<ActionResult<ImportTask>> RestartImportTask(Guid taskId)
        {
            var task = await _context.MediaImportTasks
                .Include(t => t.MediaToProcess)
                .SingleOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
            {
                return NotFound();
            }

            if (task.Status == MediaImportTaskStatus.Completed ||
                task.Status == MediaImportTaskStatus.Failed)
            {
                return BadRequest("Cannot restart a completed or failed import task. Please create a new import task.");
            }

            // Re-queue the import task for processing
            await MediaImportQueue.PushAsync(task.Id);

            return ModelToContract(task);
        }

        [HttpGet("import-status/{taskId}")]
        [Authorize]
        public async Task<ActionResult<ImportTask>> GetImportStatus(Guid taskId)
        {
            var task = await _context.MediaImportTasks
                .Include(t => t.MediaToProcess)
                .SingleOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
            {
                return NotFound();
            }
            return ModelToContract(task);
        }

        [HttpGet("import-tasks")]
        [Authorize]
        public async Task<ActionResult<List<ImportTask>>> GetImportTasks(long? gameId = null, bool includeCompleted = false)
        {
            IQueryable<MediaImportTask> tasks = _context.MediaImportTasks.Include(t => t.MediaToProcess);

            if (gameId.HasValue)
            {
                tasks = tasks.Where(t => t.Game != null && t.Game.Id == gameId);
            }

            if (!includeCompleted)
            {
                tasks = tasks
                    .Where(t => t.Status != MediaImportTaskStatus.Completed &&
                                t.Status != MediaImportTaskStatus.Failed);
            }

            return await tasks
                .Select(t => ModelToContract(t))
                .ToListAsync();
        }

        private async Task<ImportTask> SaveImportTask(List<MediaImportInfo> mediaToProcess, Game game)
        {
            var importTask = new MediaImportTask
            {
                Status = MediaImportTaskStatus.Queued,
                Game = game,
                MediaToProcess = mediaToProcess
            };
            _context.MediaImportTasks.Add(importTask);
            await _context.SaveChangesAsync();
            return ModelToContract(importTask);
        }

        private static ImportTask ModelToContract(MediaImportTask task)
        {
            int totalFiles = task.MediaToProcess.Count;
            int processedFiles = task.MediaToProcess.Count(m => m.Status == MediaImportTaskStatus.Completed);
            decimal progress = totalFiles > 0 ? (decimal)processedFiles / totalFiles : 0;
            int photoCount = task.MediaToProcess.Count(m => m.ResourceType == MediaResourceType.Photo);
            int videoCount = task.MediaToProcess.Count(m => m.ResourceType == MediaResourceType.Video);
            int livePhotoCount = task.MediaToProcess.Count(m => m.ResourceType == MediaResourceType.LivePhoto);
            string message;
            if (task.Status == MediaImportTaskStatus.Completed)
            {
                message = $"Imported {Pluralize(photoCount, "photo")}, {Pluralize(videoCount, "video")}, and {Pluralize(livePhotoCount, "live photo")}";
            }
            else if (task.Status == MediaImportTaskStatus.Failed)
            {
                message = "Import failed";
            }
            else
            {
                message = $"Importing {Pluralize(photoCount, "photo")}, {Pluralize(videoCount, "video")}, and {Pluralize(livePhotoCount, "live photo")}";
            }
            return new ImportTask
            {
                Id = task.Id,
                Status = task.Status,
                Progress = progress,
                Message = message,
                StartTime = task.StartedAt,
                EndTime = task.CompletedAt
            };
        }

        private static string Pluralize(int count, string singular)
        {
            return count == 1 ? $"{count} {singular}" : $"{count} {singular.Pluralize()}";
        }
    }
}
