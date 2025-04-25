using System.Reflection.Metadata;

namespace BaseballApi.Media;

public record class ThumbnailSize
{
    public static ThumbnailSize Small { get; } = new ThumbnailSize { MaxSize = 120, Modifier = "small" };
    public static ThumbnailSize Medium { get; } = new ThumbnailSize { MaxSize = 400, Modifier = "medium" };
    public static ThumbnailSize Large { get; } = new ThumbnailSize { MaxSize = 1600, Modifier = "large" };

    public static ThumbnailSize FromModifier(string modifier)
    {
        return modifier switch
        {
            "small" => Small,
            "medium" => Medium,
            "large" => Large,
            _ => throw new ArgumentOutOfRangeException(nameof(modifier), "Invalid thumbnail size")
        };
    }

    public int MaxSize { get; set; }
    public required string Modifier { get; set; }
}
