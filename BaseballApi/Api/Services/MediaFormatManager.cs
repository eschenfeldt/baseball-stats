using BaseballApi.Contracts;
using BaseballApi.Import;
using BaseballApi.Media;
using BaseballApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Services;

public struct ContentTypeResult
{
    public int SetCount { get; set; }
    public int UpdateCount { get; set; }
    public string? ErrorMessage { get; set; }
}

public struct AlternateFormatResult
{
    public int Count { get; set; }
    public string? ErrorMessage { get; set; }
}

public class MediaFormatManager(
        IMediaImportQueue mediaImportQueue,
        IServiceProvider serviceProvider,
        ILogger logger,
        CancellationToken cancellationToken)
{
    private IMediaImportQueue MediaImportQueue { get; } = mediaImportQueue;
    private IServiceProvider ServiceProvider { get; } = serviceProvider;
    private ILogger Logger { get; } = logger;
    private ImageConverter ImageConverter { get; } = new ImageConverter();
    private VideoConverter VideoConverter { get; } = new VideoConverter();
    private CancellationToken CancellationToken { get; } = cancellationToken;

    public async Task<ContentTypeResult> SetContentTypes(string? fileName = null)
    {
        try
        {
            Logger.LogInformation("Setting content types for media files...");
            using var scope = ServiceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<BaseballContext>();
            var remoteFileManager = scope.ServiceProvider.GetRequiredService<IRemoteFileManager>();

            // Find all media resources that need content types set, optionally restricted to a single resource
            IQueryable<MediaResource> resources = context.MediaResources;
            if (!string.IsNullOrEmpty(fileName))
            {
                resources = resources.Where(m => m.OriginalFileName == fileName);
            }
            var filesToSet = resources
                .SelectMany(m => m.Files)
                .Include(f => f.Resource)
                .Where(f => string.IsNullOrEmpty(f.ContentType))
                .ToList();

            Logger.LogInformation("Found {Count} resources to set content types.", filesToSet.Count);
            foreach (var file in filesToSet)
            {
                Logger.LogInformation("Setting content type for {ResourceId}", file.Id);
                var fileDetail = new RemoteFileDetail(file);
                var metadata = await remoteFileManager.GetFileMetadata(fileDetail);
                file.ContentType = metadata.Headers.ContentType;
            }

            await context.SaveChangesAsync();
            Logger.LogInformation("Content types set for {Count} resources.", filesToSet.Count);

            // Now find files that have the wrong content type in the bucket
            var filesToUpdate = resources
                .SelectMany(m => m.Files)
                .Where(f => f.Extension == ".mov" && f.ContentType == "binary/octet-stream") // Firefox doesn't like this (fake) content type
                .Include(f => f.Resource)
                .ToList();

            Logger.LogInformation("Found {Count} files with incorrect content type.", filesToUpdate.Count);
            foreach (var file in filesToUpdate)
            {
                Logger.LogInformation("Updating content type for {FileId}", file.Id);
                var fileDetail = new RemoteFileDetail(file);
                var metadata = await remoteFileManager.GetFileMetadata(fileDetail);
                file.ContentType = GetCorrectContentType(fileDetail);
                var response = await remoteFileManager.UpdateFileContentType(fileDetail, file.ContentType);
                var updatedMetadata = await remoteFileManager.GetFileMetadata(fileDetail);
                file.ContentType = updatedMetadata.Headers.ContentType;
                Logger.LogInformation("Updated content type for {FileId} to {ContentType}", file.Id, file.ContentType);
                await context.SaveChangesAsync();
            }
            Logger.LogInformation("Content types updated for {Count} files with incorrect content type.", filesToUpdate.Count);
            return new ContentTypeResult
            {
                SetCount = filesToSet.Count,
                UpdateCount = filesToUpdate.Count
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while setting content types.");
            return new ContentTypeResult
            {
                SetCount = 0,
                UpdateCount = 0,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AlternateFormatResult> CreateAlternateFormats(string? fileName = null)
    {
        try
        {
            if (MediaImportQueue.ImportInProgress)
            {
                Logger.LogInformation("Skipping alternate format creation, import is in progress.");
                return new()
                {
                    ErrorMessage = "Skipping alternate format creation, import is in progress."
                };
            }

            Logger.LogInformation("Creating alternate formats for media files...");
            using var scope = ServiceProvider.CreateScope();
            var remoteFileManager = scope.ServiceProvider.GetRequiredService<IRemoteFileManager>();
            using var context = scope.ServiceProvider.GetRequiredService<BaseballContext>();

            // Find up to 10 media resource that needs alternate formats created
            IQueryable<MediaResource> resources = context.MediaResources;
            if (!string.IsNullOrEmpty(fileName))
            {
                resources = resources.Where(m => m.OriginalFileName == fileName);
            }

            var resourcesToProcess = await resources
                // Filter for resources that have MOV or HEIC files and need alternate formats
                .Where(m => m.Files.Count(f => f.ContentType == "video/quicktime" || f.ContentType == "image/heic") > 0 &&
                            m.Files.Count(f => f.Purpose == RemoteFilePurpose.AlternateFormat) < m.Files.Count(f => f.Purpose == RemoteFilePurpose.Original))
                .Take(10)
                .ToListAsync(cancellationToken: CancellationToken);

            foreach (var resourceToProcess in resourcesToProcess)
            {
                Logger.LogInformation("Creating alternate formats for resource {ResourceId}", resourceToProcess.AssetIdentifier);
                switch (resourceToProcess.ResourceType)
                {
                    case MediaResourceType.Photo:
                        await CreateAlternatePhoto(resourceToProcess, remoteFileManager);
                        break;
                    case MediaResourceType.Video:
                        await CreateAlternateVideo(resourceToProcess, remoteFileManager);
                        break;
                    case MediaResourceType.LivePhoto:
                        await CreateAlternatePhoto(resourceToProcess, remoteFileManager);
                        await CreateAlternateVideo(resourceToProcess, remoteFileManager);
                        break;
                    default:
                        Logger.LogWarning("Unsupported resource type {ResourceType} for alternate formats.", resourceToProcess.ResourceType);
                        break;
                }
                await context.SaveChangesAsync();
                Logger.LogInformation("Finished creating alternate formats for resource {ResourceId}", resourceToProcess.AssetIdentifier);
            }

            return new()
            {
                Count = resourcesToProcess.Count
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while creating alternate formats.");
            return new()
            {
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task CreateAlternatePhoto(MediaResource mediaResource, IRemoteFileManager remoteFileManager)
    {
        var originalFileModel = mediaResource.Files.FirstOrDefault(f => f.Purpose == RemoteFilePurpose.Original && f.ContentType != null && f.ContentType.StartsWith("image/"));
        if (originalFileModel == null)
        {
            Logger.LogWarning("No original photo file found for resource {ResourceId}", mediaResource.AssetIdentifier);
            return;
        }
        var alternatePhoto = mediaResource.Files.FirstOrDefault(f => f.Purpose == RemoteFilePurpose.AlternateFormat && f.ContentType != null && f.ContentType.StartsWith("image/"));
        if (alternatePhoto != null)
        {
            Logger.LogInformation("Alternate photo already exists for resource {ResourceId}", mediaResource.AssetIdentifier);
            return;
        }

        var originalFileDetail = new RemoteFileDetail(originalFileModel);
        var originalFile = await DownloadRemoteFile(originalFileDetail, remoteFileManager);
        if (string.IsNullOrEmpty(originalFile))
        {
            Logger.LogWarning("Could not download original file for resource {ResourceId}", mediaResource.AssetIdentifier);
            return;
        }
        var altPhoto = ImageConverter.CreateJpeg(new FileInfo(originalFile), null);
        if (altPhoto == null)
        {
            Logger.LogWarning("Failed to create alternate photo for resource {ResourceId}", mediaResource.AssetIdentifier);
            return;
        }
        var altPhotoFile = new RemoteFile
        {
            Resource = mediaResource,
            Purpose = RemoteFilePurpose.AlternateFormat,
            Extension = altPhoto.Extension,
        };
        await remoteFileManager.UploadFile(altPhotoFile, altPhoto.FullName);
        var altMetadata = await remoteFileManager.GetFileMetadata(new RemoteFileDetail(altPhotoFile));
        altPhotoFile.ContentType = altMetadata.Headers.ContentType;
        mediaResource.Files.Add(altPhotoFile);

        Logger.LogInformation("Created alternate photo for resource {ResourceId} with content type {ContentType}", mediaResource.AssetIdentifier, altPhotoFile.ContentType);
        // Clean up the temporary files
        if (File.Exists(originalFile))
        {
            File.Delete(originalFile);
        }
        if (File.Exists(altPhoto.FullName))
        {
            File.Delete(altPhoto.FullName);
        }
    }

    private async Task CreateAlternateVideo(MediaResource mediaResource, IRemoteFileManager remoteFileManager)
    {
        var originalFileModel = mediaResource.Files.FirstOrDefault(f => f.Purpose == RemoteFilePurpose.Original && f.ContentType != null
                                                                    && f.ContentType.StartsWith("video/") || f.ContentType == "application/octet-stream");
        if (originalFileModel == null)
        {
            Logger.LogWarning("No original video file found for resource {ResourceId}", mediaResource.Id);
            return;
        }
        var alternateVideo = mediaResource.Files.FirstOrDefault(f => f.Purpose == RemoteFilePurpose.AlternateFormat && f.ContentType != null && f.ContentType.StartsWith("video/"));
        if (alternateVideo != null)
        {
            Logger.LogInformation("Alternate photo already exists for resource {ResourceId}", mediaResource.AssetIdentifier);
            return;
        }

        var originalFileDetail = new RemoteFileDetail(originalFileModel);
        var originalFile = await DownloadRemoteFile(originalFileDetail, remoteFileManager);
        if (string.IsNullOrEmpty(originalFile))
        {
            Logger.LogWarning("Could not download original file for resource {ResourceId}", mediaResource.AssetIdentifier);
            return;
        }
        var videoFile = new FileInfo(originalFile);
        var videoInfo = VideoConverter.GetVideoInfo(videoFile);
        if (videoInfo == null)
        {
            throw new ArgumentException($"Failed to process video file {originalFile}");
        }
        VideoStreamInfo convertedVideoStream = videoInfo.Streams[0];
        FileInfo? altVideo = null;
        if (convertedVideoStream.CodecName != "h264")
        {
            altVideo = VideoConverter.ConvertVideo(videoFile);
            if (altVideo == null)
            {
                Logger.LogError("Failed to convert video file {originalFile} to H264", originalFile);
            }
            else
            {
                var altVideoFile = new RemoteFile
                {
                    Resource = mediaResource,
                    Purpose = RemoteFilePurpose.AlternateFormat,
                    Extension = altVideo.Extension,
                };
                await remoteFileManager.UploadFile(altVideoFile, altVideo.FullName);
                var altMetadata = await remoteFileManager.GetFileMetadata(new RemoteFileDetail(altVideoFile));
                altVideoFile.ContentType = altMetadata.Headers.ContentType;
                mediaResource.Files.Add(altVideoFile);
                Logger.LogInformation("Created alternate video for resource {ResourceId} with content type {ContentType}", mediaResource.AssetIdentifier, altVideoFile.ContentType);

                if (File.Exists(altVideo.FullName))
                {
                    File.Delete(altVideo.FullName);
                }
            }
        }

        // Clean up the temporary files
        if (File.Exists(originalFile))
        {
            File.Delete(originalFile);
        }
    }

    private static async Task<string> DownloadRemoteFile(RemoteFileDetail fileDetail, IRemoteFileManager remoteFileManager)
    {
        var response = await remoteFileManager.GetFile(fileDetail);
        if (response == null || response.ResponseStream == null)
        {
            throw new InvalidOperationException($"Failed to download file: {fileDetail.Key}");
        }

        var tempFilePath = Path.Combine(Path.GetTempPath(), fileDetail.Key.Replace("/", "_"));
        using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
        {
            await response.ResponseStream.CopyToAsync(fileStream);
        }
        return tempFilePath;
    }

    private static string GetCorrectContentType(RemoteFileDetail fileDetail)
    {
        if (string.IsNullOrEmpty(fileDetail.Extension))
        {
            return "application/octet-stream"; // Default content type
        }
        var extension = fileDetail.Extension.ToLowerInvariant();
        return extension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".heic" => "image/heic",
            ".png" => "image/png",
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}