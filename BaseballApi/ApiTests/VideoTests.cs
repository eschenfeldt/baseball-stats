using BaseballApi.Media;

namespace BaseballApiTests;

public class VideoTests
{
    [Theory]
    [Trait(TestCategory.Category, TestCategory.Media)]
    [InlineData("hevc.mov", "hevc")]
    public void TestConversion(string fileName, string expectedOriginalCodecName)
    {
        FileInfo original = new(Path.Join("data", "media", "video", fileName));
        VideoConverter converter = new();

        VideoInfo originalInfo = converter.GetVideoInfo(original);
        Assert.Equal(5, originalInfo.Streams.Count);
        VideoStreamInfo videoStream = originalInfo.Streams[0];
        Assert.Equal("video", videoStream.CodecType);
        Assert.Equal(expectedOriginalCodecName, videoStream.CodecName);

        FileInfo converted = converter.ConvertVideo(original);

        VideoInfo convertedInfo = converter.GetVideoInfo(converted);
        Assert.Equal(2, convertedInfo.Streams.Count);
        VideoStreamInfo convertedVideoStream = convertedInfo.Streams[0];
        Assert.Equal("video", convertedVideoStream.CodecType);
        Assert.Equal("h264", convertedVideoStream.CodecName);

        converted.Delete();
        Assert.False(converted.Exists);
    }

    [Theory]
    [Trait(TestCategory.Category, TestCategory.Media)]
    [InlineData("video", "hevc.mov", "small")]
    [InlineData("video", "hevc.mov", "medium")]
    [InlineData("video", "hevc.mov", "large")]
    public void TestVideoThumbnailing(string folderName, string fileName, string sizeModifier)
    {
        FileInfo original = new(Path.Join("data", "media", folderName, fileName));
        VideoConverter converter = new();
        ImageConverter imageConverter = new();

        var originalInfo = converter.GetVideoInfo(original);
        Assert.NotNull(originalInfo);
        Assert.Equal(5, originalInfo.Streams.Count);
        VideoStreamInfo videoStream = originalInfo.Streams[0];
        Assert.Equal("video", videoStream.CodecType);
        Assert.True(videoStream.Width > 0);
        Assert.True(videoStream.Height > 0);

        var size = ThumbnailSize.FromModifier(sizeModifier);

        FileInfo frame = converter.CreateJpeg(original);
        Assert.True(frame.Exists);
        Assert.True(frame.Length > 0);
        var frameInfo = imageConverter.GetImageInfo(frame);
        Assert.Equal(videoStream.Width, frameInfo.Width);
        Assert.Equal(videoStream.Height, frameInfo.Height);

        FileInfo thumbnail = imageConverter.CreateJpeg(frame, size);
        Assert.True(thumbnail.Exists);
        Assert.True(thumbnail.Length > 0);
        Assert.Contains(size.Modifier, thumbnail.Name);

        ImageInfo thumbnailInfo = imageConverter.GetImageInfo(thumbnail);
        Assert.Equal("jpeg", thumbnailInfo.Extension);
        Assert.True(thumbnailInfo.Width > 0);
        Assert.True(thumbnailInfo.Width <= videoStream.Width);
        Assert.True(thumbnailInfo.Width <= size.MaxSize);
        Assert.True(thumbnailInfo.Height > 0);
        Assert.True(thumbnailInfo.Height <= videoStream.Height);
        Assert.True(thumbnailInfo.Height <= size.MaxSize);

        Assert.True(thumbnailInfo.Width <= videoStream.Width);
        Assert.True(thumbnailInfo.Height <= videoStream.Height);

        decimal originalAspectRatio = (decimal)videoStream.Width / videoStream.Height;
        decimal thumbnailAspectRatio = (decimal)thumbnailInfo.Width / thumbnailInfo.Height;
        Assert.Equal(originalAspectRatio, thumbnailAspectRatio, 1);

        thumbnail.Delete();
        Assert.False(thumbnail.Exists);
    }
}
