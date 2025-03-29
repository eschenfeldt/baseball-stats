using System;
using BaseballApi.Import;
using BaseballApi.Video;

namespace BaseballApiTests;

public class VideoTests
{
    [Theory]
    [InlineData("hevc.MOV", "hevc")]
    public void TestConversion(string fileName, string expectedOriginalCodecName)
    {
        FileInfo original = new(Path.Join("data", fileName));
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
    }
}
