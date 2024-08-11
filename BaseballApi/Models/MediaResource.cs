using System;

namespace BaseballApi.Models;

public class MediaResource : RemoteResource
{
    public required MediaResourceType ResourceType { get; set; }

    public ICollection<Player> Players { get; } = [];
}
