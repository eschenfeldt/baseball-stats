using BaseballApi.Contracts;
using BaseballApi.Import;
using BaseballApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Media;

public class MediaImportManager(List<MediaImportInfo> resources, IRemoteFileManager remoteFileManager, BaseballContext context, Game? game = null)
{
    BaseballContext Context { get; } = context;
    Game? Game { get; } = game;
    List<MediaImportInfo> Resources { get; } = resources;
    private IRemoteFileManager RemoteFileManager { get; } = remoteFileManager;
    private VideoConverter VideoConverter { get; } = new VideoConverter();
    private ImageConverter ImageConverter { get; } = new ImageConverter();

    public async IAsyncEnumerable<MediaUploadResult> GetUploadedResources()
    {
        foreach (var resource in Resources)
        {
            if (!await ResourceExists(resource))
            {
                switch (resource.ResourceType)
                {
                    case MediaResourceType.Scorecard:
                        throw new NotSupportedException("Scorecard import is not supported via this endpoint");
                    case MediaResourceType.Photo:
                        yield return await ProcessPhoto(resource);
                        break;
                    case MediaResourceType.LivePhoto:
                        yield return await ProcessLivePhoto(resource);
                        break;
                    case MediaResourceType.Video:
                        yield return await ProcessVideo(resource);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(resource.ResourceType), "Invalid media resource type");
                }
            }
        }
    }

    private async Task<bool> ResourceExists(MediaImportInfo resource)
    {
        var gameId = Game?.Id;
        var fileName = resource.ResourceType == MediaResourceType.Video ? resource.VideoFileName : resource.PhotoFileName;
        return await Context.MediaResources.AnyAsync(r => r.OriginalFileName == fileName && r.Game != null && r.Game.Id == gameId);
    }

    private async Task<MediaUploadResult> ProcessPhoto(MediaImportInfo resource)
    {
        if (string.IsNullOrEmpty(resource.PhotoFilePath))
        {
            throw new ArgumentException("Photo must have a photo file path");
        }
        if (string.IsNullOrEmpty(resource.PhotoFileName))
        {
            throw new ArgumentException("Photo must have a photo file name");
        }
        var mediaResource = new MediaResource
        {
            AssetIdentifier = Guid.NewGuid(),
            ResourceType = MediaResourceType.Photo,
            OriginalFileName = resource.PhotoFileName
        };

        resource.Status = MediaImportTaskStatus.InProgress;
        resource.StartedAt = DateTimeOffset.UtcNow;
        await Context.SaveChangesAsync();

        try
        {
            await ProcessPhotoInternal(mediaResource, resource.PhotoFilePath, resource.PhotoFileName);
        }
        catch (Exception ex)
        {
            resource.Status = MediaImportTaskStatus.Failed;
            await Context.SaveChangesAsync();
            return new MediaUploadResult
            {
                OriginalResource = resource,
                ErrorMessage = ex.Message
            };
        }

        resource.Status = MediaImportTaskStatus.Completed;
        resource.CompletedAt = DateTimeOffset.UtcNow;

        return new MediaUploadResult
        {
            OriginalResource = resource,
            Resource = mediaResource
        };
    }

    private async Task<MediaUploadResult> ProcessVideo(MediaImportInfo resource)
    {
        if (string.IsNullOrEmpty(resource.VideoFilePath))
        {
            throw new ArgumentException("Video must have a video file path");
        }
        if (string.IsNullOrEmpty(resource.VideoFileName))
        {
            throw new ArgumentException("Video must have a video file name");
        }
        var mediaResource = new MediaResource
        {
            AssetIdentifier = Guid.NewGuid(),
            ResourceType = MediaResourceType.Video,
            OriginalFileName = resource.VideoFileName
        };

        resource.Status = MediaImportTaskStatus.InProgress;
        resource.StartedAt = DateTimeOffset.UtcNow;
        await Context.SaveChangesAsync();

        try
        {
            await ProcessVideoInternal(mediaResource, resource.VideoFilePath, resource.VideoFileName);
        }
        catch (Exception ex)
        {
            resource.Status = MediaImportTaskStatus.Failed;
            await Context.SaveChangesAsync();
            return new MediaUploadResult
            {
                OriginalResource = resource,
                ErrorMessage = ex.Message
            };
        }

        resource.Status = MediaImportTaskStatus.Completed;
        resource.CompletedAt = DateTimeOffset.UtcNow;

        return new MediaUploadResult
        {
            OriginalResource = resource,
            Resource = mediaResource
        };
    }

    private async Task<MediaUploadResult> ProcessLivePhoto(MediaImportInfo resource)
    {
        if (string.IsNullOrEmpty(resource.PhotoFilePath) || string.IsNullOrEmpty(resource.VideoFilePath))
        {
            throw new ArgumentException("Live photo must have both a photo and video file path");
        }
        if (string.IsNullOrEmpty(resource.PhotoFileName) || string.IsNullOrEmpty(resource.VideoFileName))
        {
            throw new ArgumentException("Live photo must have both a photo and video file name");
        }
        var mediaResource = new MediaResource
        {
            AssetIdentifier = Guid.NewGuid(),
            ResourceType = MediaResourceType.LivePhoto,
            OriginalFileName = resource.PhotoFileName
        };

        resource.Status = MediaImportTaskStatus.InProgress;
        resource.StartedAt = DateTimeOffset.UtcNow;
        await Context.SaveChangesAsync();

        try
        {
            await ProcessPhotoInternal(mediaResource, resource.PhotoFilePath, resource.PhotoFileName);
            await ProcessVideoInternal(mediaResource, resource.VideoFilePath, resource.VideoFileName);
        }
        catch (Exception ex)
        {
            resource.Status = MediaImportTaskStatus.Failed;
            await Context.SaveChangesAsync();
            return new MediaUploadResult
            {
                OriginalResource = resource,
                ErrorMessage = ex.Message
            };
        }

        resource.Status = MediaImportTaskStatus.Completed;
        resource.CompletedAt = DateTimeOffset.UtcNow;

        return new MediaUploadResult
        {
            OriginalResource = resource,
            Resource = mediaResource
        };
    }

    private async Task ProcessPhotoInternal(MediaResource mediaResource, string photoFilePath, string photoFileName)
    {
        if (string.IsNullOrEmpty(photoFilePath))
        {
            throw new ArgumentException("Photo must have a photo file path");
        }
        if (string.IsNullOrEmpty(photoFileName))
        {
            throw new ArgumentException("Photo must have a photo file name");
        }

        var photoFile = new FileInfo(photoFilePath);
        var photoInfo = ImageConverter.GetImageInfo(photoFile);
        if (photoInfo == null)
        {
            throw new ArgumentException($"Failed to process photo file {photoFileName}");
        }

        var exifInfo = ImageConverter.GetExifInfo(photoFile);
        if (exifInfo == null)
        {
            throw new ArgumentException($"Failed to read exif data from photo file {photoFileName}");
        }
        mediaResource.DateTime = exifInfo.CreationDate.ToUniversalTime(); // Postgres must be UTC

        FileInfo? altPhoto = null;
        if (photoInfo.Extension != ".jpg" && photoInfo.Extension != ".jpeg")
        {
            altPhoto = ImageConverter.CreateJpeg(new FileInfo(photoFilePath), null);
            if (altPhoto == null)
            {
                throw new ArgumentException($"Failed to convert photo file {photoFileName} to JPEG");
            }
        }

        var originalPhoto = new RemoteFile
        {
            Resource = mediaResource,
            Purpose = RemoteFilePurpose.Original,
            Extension = photoFile.Extension
        };
        await RemoteFileManager.UploadFile(originalPhoto, photoFilePath);
        // Get the content type from the bucket so we have it in the database
        var metadata = await RemoteFileManager.GetFileMetadata(new RemoteFileDetail(originalPhoto));
        originalPhoto.ContentType = metadata.Headers.ContentType;
        mediaResource.Files.Add(originalPhoto);

        if (altPhoto != null)
        {
            var altPhotoFile = new RemoteFile
            {
                Resource = mediaResource,
                Purpose = RemoteFilePurpose.AlternateFormat,
                Extension = altPhoto.Extension
            };
            await RemoteFileManager.UploadFile(altPhotoFile, altPhoto.FullName);
            var altMetadata = await RemoteFileManager.GetFileMetadata(new RemoteFileDetail(altPhotoFile));
            altPhotoFile.ContentType = altMetadata.Headers.ContentType;
            mediaResource.Files.Add(altPhotoFile);
        }
        await GeneratePhotoThumbnails(mediaResource, altPhoto ?? new FileInfo(photoFilePath));
    }

    private async Task ProcessVideoInternal(MediaResource mediaResource, string videoFilePath, string videoFileName)
    {
        var videoFile = new FileInfo(videoFilePath);
        if (mediaResource.DateTime == default)
        {
            // If the date time is not set, this isn't a live photo, so we should set it from the video file
            var exifInfo = ImageConverter.GetExifInfo(videoFile);
            if (exifInfo == null)
            {
                throw new ArgumentException($"Failed to read exif data from video file {videoFileName}");
            }
            mediaResource.DateTime = exifInfo.CreationDate.ToUniversalTime(); // Postgres must be UTC
        }

        var videoInfo = VideoConverter.GetVideoInfo(videoFile);
        if (videoInfo == null)
        {
            throw new ArgumentException($"Failed to process video file {videoFileName}");
        }
        VideoStreamInfo convertedVideoStream = videoInfo.Streams[0];
        FileInfo? altVideo = null;
        if (convertedVideoStream.CodecName != "h264")
        {
            altVideo = VideoConverter.ConvertVideo(videoFile);
            if (altVideo == null)
            {
                throw new ArgumentException($"Failed to convert video file {videoFileName} to H264");
            }
        }

        var originalVideo = new RemoteFile
        {
            Resource = mediaResource,
            Purpose = RemoteFilePurpose.Original,
            Extension = Path.GetExtension(videoFileName)
        };
        await RemoteFileManager.UploadFile(originalVideo, videoFilePath);
        // Get the content type from the bucket so we have it in the database
        var metadata = await RemoteFileManager.GetFileMetadata(new RemoteFileDetail(originalVideo));
        originalVideo.ContentType = metadata.Headers.ContentType;
        mediaResource.Files.Add(originalVideo);

        if (altVideo != null)
        {
            var altVideoFile = new RemoteFile
            {
                Resource = mediaResource,
                Purpose = RemoteFilePurpose.AlternateFormat,
                Extension = Path.GetExtension(altVideo.FullName)
            };
            await RemoteFileManager.UploadFile(altVideoFile, altVideo.FullName);
            var altMetadata = await RemoteFileManager.GetFileMetadata(new RemoteFileDetail(altVideoFile));
            altVideoFile.ContentType = altMetadata.Headers.ContentType;
            mediaResource.Files.Add(altVideoFile);
        }

        if (mediaResource.ResourceType == MediaResourceType.Video)
        {
            // live photo will get its thumbnails from the photo
            var frame = VideoConverter.CreateJpeg(videoFile);
            await this.GeneratePhotoThumbnails(mediaResource, frame);
        }
    }

    private async Task GeneratePhotoThumbnails(MediaResource mediaResource, FileInfo photo)
    {
        List<ThumbnailSize> thumbnailSizes = [ThumbnailSize.Small, ThumbnailSize.Medium, ThumbnailSize.Large];
        foreach (var size in thumbnailSizes)
        {
            var thumbnail = ImageConverter.CreateJpeg(photo, size);
            if (thumbnail != null)
            {
                var thumbnailFile = new RemoteFile
                {
                    Resource = mediaResource,
                    Purpose = RemoteFilePurpose.Thumbnail,
                    Extension = thumbnail.Extension,
                    NameModifier = size.Modifier
                };
                await RemoteFileManager.UploadFile(thumbnailFile, thumbnail.FullName);
                var thumbnailMetadata = await RemoteFileManager.GetFileMetadata(new RemoteFileDetail(thumbnailFile));
                thumbnailFile.ContentType = thumbnailMetadata.Headers.ContentType;
                mediaResource.Files.Add(thumbnailFile);
            }
        }
    }
}
