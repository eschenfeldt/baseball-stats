using System;
using Amazon.S3;
using Amazon.S3.Model;
using BaseballApi.Contracts;
using BaseballApi.Models;

namespace BaseballApi.Import;

public class RemoteFileManager : IRemoteFileManager
{
    AmazonS3Client Client { get; }
    string BucketName { get; }
    string? KeyPrefix { get; }

    public RemoteFileManager(string? keyPrefix = null)
    {
        this.KeyPrefix = keyPrefix;
        var builder = new ConfigurationBuilder().AddUserSecrets<RemoteFileManager>();
        IConfiguration configuration = builder.Build();
        var accessKey = configuration["Spaces:AccessKey"];
        var secretKey = configuration["Spaces:SecretKey"];

        AmazonS3Config config = new()
        {
            ServiceURL = "https://nyc3.digitaloceanspaces.com"
        };

        this.Client = new AmazonS3Client(
            accessKey,
            secretKey,
            config
        );
        this.BucketName = configuration["Spaces:Bucket"] ?? "eschenfeldt-baseball-media";
    }

    private string GetKey(RemoteFileDetail remoteFileDetail)
    {
        var key = remoteFileDetail.Key;
        if (!string.IsNullOrWhiteSpace(this.KeyPrefix))
        {
            key = this.KeyPrefix + "/" + key;
        }
        return key;
    }

    public async Task<PutObjectResponse> UploadFile(RemoteFile file, string filePath)
    {
        using FileStream fileStream = File.OpenRead(filePath);
        RemoteFileDetail fileDetail = new(file);
        PutObjectRequest request = new()
        {
            BucketName = this.BucketName,
            Key = this.GetKey(fileDetail),
            InputStream = fileStream,
            CannedACL = S3CannedACL.PublicRead
        };
        return await this.Client.PutObjectAsync(request);
    }

    public async Task<DeleteObjectsResponse> DeleteResource(RemoteResource resource)
    {
        DeleteObjectsRequest request = new()
        {
            BucketName = this.BucketName,
            Objects = resource.Files
                .Select(f => this.GetKey(new RemoteFileDetail(f)))
                .Select(k => new KeyVersion { Key = k }).ToList()
        };
        return await this.Client.DeleteObjectsAsync(request);
    }

    public async Task<GetObjectMetadataResponse> GetFileMetadata(RemoteFileDetail fileDetail)
    {
        var request = new GetObjectMetadataRequest
        {
            BucketName = this.BucketName,
            Key = this.GetKey(fileDetail)
        };
        return await this.Client.GetObjectMetadataAsync(request);
    }
}
