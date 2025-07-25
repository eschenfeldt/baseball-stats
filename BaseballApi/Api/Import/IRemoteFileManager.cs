using System;
using Amazon.S3.Model;
using BaseballApi.Contracts;
using BaseballApi.Models;

namespace BaseballApi.Import;

public interface IRemoteFileManager
{
    public Task<PutObjectResponse> UploadFile(RemoteFile file, string filePath);
    public Task<DeleteObjectsResponse> DeleteResource(RemoteResource resource);
    public Task<GetObjectMetadataResponse> GetFileMetadata(RemoteFileDetail fileDetail);
    public Task<CopyObjectResponse> UpdateFileContentType(RemoteFileDetail fileDetail, string contentType);
}
