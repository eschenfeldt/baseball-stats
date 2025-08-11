namespace BaseballApi.Models;

public class MediaResource : RemoteResource
{
    public bool Favorite { get; set; }

    /// <summary>
    /// If true, this resource already has any required alternate formats and does not need to be processed again
    ///     even if it otherwise would qualify.
    /// If null, whether that override should apply has not been determined yet.
    /// </summary>
    public bool? AlternateFormatOverride { get; set; }

    public required MediaResourceType ResourceType { get; set; }

    public ICollection<Player> Players { get; } = [];
}