using System;
using Amazon.S3.Model;
using BaseballApi.Contracts;
using BaseballApi.Import;
using BaseballApi.Models;

namespace BaseballApiTests;

public class MockRemoteFileManager : IRemoteFileManager
{
    readonly Dictionary<string, string> ContentTypes = [];

    public Task<DeleteObjectsResponse> DeleteResource(RemoteResource resource)
    {
        throw new NotImplementedException();
    }

    public Task<GetObjectResponse> GetFile(RemoteFileDetail fileDetail)
    {
        throw new NotImplementedException();
    }

    public async Task<GetObjectMetadataResponse> GetFileMetadata(RemoteFileDetail fileDetail)
    {
        throw new NotImplementedException();
    }

    public Task<CopyObjectResponse> UpdateFileContentType(RemoteFileDetail fileDetail, string contentType)
    {
        throw new NotImplementedException();
    }

    public async Task<PutObjectResponse> UploadFile(RemoteFile file, string filePath)
    {
        RemoteFileDetail fileDetail = new(file);
        Assert.NotNull(file.ContentType);
        this.ContentTypes[fileDetail.Key] = file.ContentType;
        return new PutObjectResponse();
    }
}
