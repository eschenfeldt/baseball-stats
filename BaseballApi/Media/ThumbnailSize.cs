using System.Reflection.Metadata;

namespace BaseballApi.Media;

public record class ThumbnailSize
{
    public static ThumbnailSize Small { get; } = new ThumbnailSize { MaxSize = 120, Modifier = "small" };
    public static ThumbnailSize Medium { get; } = new ThumbnailSize { MaxSize = 400, Modifier = "medium" };
    public static ThumbnailSize Large { get; } = new ThumbnailSize { MaxSize = 1600, Modifier = "large" };

    public static ThumbnailSize FromSize(int size)
    {
        return size switch
        {
            120 => Small,
            400 => Medium,
            1600 => Large,
            _ => throw new ArgumentOutOfRangeException(nameof(size), "Invalid thumbnail size")
        };
    }

    public int MaxSize { get; set; }
    public required string Modifier { get; set; }
}
