using System;
using Amazon.S3.Model;
using BaseballApi.Contracts;
using BaseballApi.Import;
using BaseballApi.Models;

namespace BaseballApiTests;

public class MockRemoteFileManager : IRemoteFileManager
{
    HashSet<string> MockUploadedKeys { get; } = [];

    public Task<DeleteObjectsResponse> DeleteResource(RemoteResource resource)
    {
        throw new NotImplementedException();
    }

    public Task<GetObjectResponse> GetFile(RemoteFileDetail fileDetail)
    {
        throw new NotImplementedException();
    }

    public Task<GetObjectMetadataResponse> GetFileMetadata(RemoteFileDetail fileDetail)
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
        this.MockUploadedKeys.Add(fileDetail.Key);
        return new PutObjectResponse();
    }
}
