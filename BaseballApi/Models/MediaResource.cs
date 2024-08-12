using System;

namespace BaseballApi.Models;

public class MediaResource : RemoteResource
{
    public bool Favorite { get; set; }

    public required MediaResourceType ResourceType { get; set; }

    public ICollection<Player> Players { get; } = [];
}