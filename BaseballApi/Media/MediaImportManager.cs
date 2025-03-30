using BaseballApi.Import;
using BaseballApi.Models;

namespace BaseballApi.Media;

public class MediaImportManager(List<MediaImportInfo> resources, IRemoteFileManager remoteFileManager)
{
    List<MediaImportInfo> Resources { get; } = resources;
    private IRemoteFileManager RemoteFileManager { get; } = remoteFileManager;
    private VideoConverter VideoConverter { get; } = new VideoConverter();
    private ImageConverter ImageConverter { get; } = new ImageConverter();

    public async IAsyncEnumerable<MediaResource> GetUploadedResources()
    {
        foreach (var resource in Resources)
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

    private async Task<MediaResource> ProcessPhoto(MediaImportInfo resource)
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
            ResourceType = MediaResourceType.Photo,
            OriginalFileName = resource.PhotoFileName
        };

        await ProcessPhotoInternal(mediaResource, resource.PhotoFilePath, resource.PhotoFileName);

        return mediaResource;
    }

    private async Task<MediaResource> ProcessVideo(MediaImportInfo resource)
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
            ResourceType = MediaResourceType.Video,
            OriginalFileName = resource.VideoFileName
        };

        await ProcessVideoInternal(mediaResource, resource.VideoFilePath, resource.VideoFileName);

        return mediaResource;
    }

    private async Task<MediaResource> ProcessLivePhoto(MediaImportInfo resource)
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
            ResourceType = MediaResourceType.LivePhoto,
            OriginalFileName = resource.PhotoFileName
        };

        await ProcessPhotoInternal(mediaResource, resource.PhotoFilePath, resource.PhotoFileName);
        await ProcessVideoInternal(mediaResource, resource.VideoFilePath, resource.VideoFileName);

        return mediaResource;
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

        var photoInfo = ImageConverter.GetImageInfo(new FileInfo(photoFilePath));
        if (photoInfo == null)
        {
            throw new ArgumentException($"Failed to process photo file {photoFileName}");
        }

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
            Extension = photoInfo.Extension
        };
        await RemoteFileManager.UploadFile(originalPhoto, photoFilePath);
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
            mediaResource.Files.Add(altPhotoFile);
        }
        await GeneratePhotoThumbnails(mediaResource, altPhoto ?? new FileInfo(photoFilePath));
    }

    private async Task ProcessVideoInternal(MediaResource mediaResource, string videoFilePath, string videoFileName)
    {
        var videoInfo = VideoConverter.GetVideoInfo(new FileInfo(videoFilePath));
        if (videoInfo == null)
        {
            throw new ArgumentException($"Failed to process video file {videoFileName}");
        }
        VideoStreamInfo convertedVideoStream = videoInfo.Streams[0];
        FileInfo? altVideo = null;
        if (convertedVideoStream.CodecName != "h264")
        {
            var convertedVideo = VideoConverter.ConvertVideo(new FileInfo(videoFilePath));
            if (convertedVideo == null)
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
            mediaResource.Files.Add(altVideoFile);
        }
        // TODO: generate video thumbnails
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
                    Extension = thumbnail.Extension
                };
                await RemoteFileManager.UploadFile(thumbnailFile, thumbnail.FullName);
                mediaResource.Files.Add(thumbnailFile);
            }
        }
    }
}
