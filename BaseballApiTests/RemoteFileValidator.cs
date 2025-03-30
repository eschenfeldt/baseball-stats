using Amazon.S3;
using BaseballApi.Contracts;
using BaseballApi.Import;

namespace BaseballApiTests;

public class RemoteFileValidator
{
    RemoteFileManager Manager { get; }

    public RemoteFileValidator(RemoteFileManager fileManager)
    {
        this.Manager = fileManager;
    }

    public async Task ValidateFileExists(RemoteFileDetail fileDetail)
    {
        var metadata = await Manager.GetFileMetadata(fileDetail);
        Assert.NotNull(metadata);
        Assert.NotEqual(0, metadata.ContentLength);
    }

    public async Task ValidateFileDeleted(RemoteFileDetail fileDetail)
    {
        try
        {
            await Manager.GetFileMetadata(fileDetail);
        }
        catch (AmazonS3Exception e)
        {
            Assert.Equal("NotFound", e.ErrorCode);
            return;
        }
        Assert.Fail("Should have triggered NotFound exception");
    }
}
