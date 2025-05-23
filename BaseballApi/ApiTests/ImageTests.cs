using BaseballApi.Media;

namespace BaseballApiTests;

public class ImageTests
{
    [Theory]
    [InlineData("live photos", "IMG_4762.HEIC", "small")]
    [InlineData("live photos", "IMG_4762.HEIC", "medium")]
    [InlineData("live photos", "IMG_4762.HEIC", "large")]
    [InlineData("live photos", "IMG_4771.HEIC", "small")]
    [InlineData("live photos", "IMG_4771.HEIC", "medium")]
    [InlineData("live photos", "IMG_4771.HEIC", "large")]
    public void TestImageThumbnailing(string folderName, string fileName, string sizeModifier)
    {
        FileInfo original = new(Path.Join("data", "media", folderName, fileName));
        ImageConverter converter = new();

        ImageInfo originalInfo = converter.GetImageInfo(original);
        Assert.Equal(original.Extension.TrimStart('.'), originalInfo.Extension);
        Assert.True(originalInfo.Width > 0);
        Assert.True(originalInfo.Height > 0);

        var size = ThumbnailSize.FromModifier(sizeModifier);

        FileInfo thumbnail = converter.CreateJpeg(original, size);
        Assert.True(thumbnail.Exists);
        Assert.True(thumbnail.Length > 0);
        Assert.Contains(size.Modifier, thumbnail.Name);

        ImageInfo thumbnailInfo = converter.GetImageInfo(thumbnail);
        Assert.Equal("jpeg", thumbnailInfo.Extension);
        Assert.True(thumbnailInfo.Width > 0);
        Assert.True(thumbnailInfo.Width <= originalInfo.Width);
        Assert.True(thumbnailInfo.Width <= size.MaxSize);
        Assert.True(thumbnailInfo.Height > 0);
        Assert.True(thumbnailInfo.Height <= originalInfo.Height);
        Assert.True(thumbnailInfo.Height <= size.MaxSize);

        Assert.True(thumbnailInfo.Width <= originalInfo.Width);
        Assert.True(thumbnailInfo.Height <= originalInfo.Height);

        decimal originalAspectRatio = (decimal)originalInfo.Width / originalInfo.Height;
        decimal thumbnailAspectRatio = (decimal)thumbnailInfo.Width / thumbnailInfo.Height;
        Assert.Equal(originalAspectRatio, thumbnailAspectRatio, 2);

        thumbnail.Delete();
        Assert.False(thumbnail.Exists);
    }

    [Theory]
    [InlineData("live photos", "IMG_4762.HEIC")]
    [InlineData("live photos", "IMG_4771.HEIC")]
    public void TestImageConversion(string folderName, string fileName)
    {
        FileInfo original = new(Path.Join("data", "media", folderName, fileName));
        ImageConverter converter = new();

        ImageInfo originalInfo = converter.GetImageInfo(original);
        Assert.Equal(original.Extension.TrimStart('.'), originalInfo.Extension);
        Assert.True(originalInfo.Width > 0);
        Assert.True(originalInfo.Height > 0);

        FileInfo converted = converter.CreateJpeg(original, null);
        Assert.True(converted.Exists);
        Assert.True(converted.Length > 0);

        ImageInfo convertedInfo = converter.GetImageInfo(converted);
        Assert.Equal("jpeg", convertedInfo.Extension);
        Assert.Equal(originalInfo.Width, convertedInfo.Width);
        Assert.Equal(originalInfo.Height, convertedInfo.Height);

        converted.Delete();
        Assert.False(converted.Exists);
    }

    [Theory]
    [InlineData("live photos", "IMG_4762.HEIC")]
    [InlineData("live photos", "IMG_4771.HEIC")]
    [InlineData("photos", "IMG_4721.HEIC")]
    [InlineData("video", "hevc.mov")]
    public void TestExifDate(string folderName, string fileName)
    {
        if (!MediaTests.ExpectedResourceTimes.TryGetValue(fileName, out DateTimeOffset expectedDateTime))
        {
            Assert.Fail($"No expected date time for {fileName}");
            return;
        }

        FileInfo original = new(Path.Join("data", "media", folderName, fileName));
        ImageConverter converter = new();

        ExifInfo exif = converter.GetExifInfo(original);
        Assert.Equal(expectedDateTime, exif.CreationDate);
    }
}
