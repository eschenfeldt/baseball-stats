namespace BaseballApi.Models;

public class MediaResource : RemoteResource
{
    public bool Favorite { get; set; }

    /// <summary>
    /// If true, this resource already has any required alternate formats and does not need to be processed again.
    /// </summary>
    public required bool AlternateFormatOverride { get; set; } = false;

    public required MediaResourceType ResourceType { get; set; }

    public ICollection<Player> Players { get; } = [];
}