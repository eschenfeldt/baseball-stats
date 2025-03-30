using BaseballApi.Models;

namespace BaseballApi.Media;

public class MediaImportManager(List<MediaImportInfo> resources)
{
    List<MediaImportInfo> Resources { get; } = resources;
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
        throw new NotImplementedException("Photo import is not implemented yet");
    }

    private async Task<MediaResource> ProcessLivePhoto(MediaImportInfo resource)
    {
        throw new NotImplementedException("Photo import is not implemented yet");
    }

    private async Task<MediaResource> ProcessVideo(MediaImportInfo resource)
    {
        throw new NotImplementedException("Photo import is not implemented yet");
    }
}
