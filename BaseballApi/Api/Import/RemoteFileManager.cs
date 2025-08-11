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

    public RemoteFileManager(IConfiguration configuration, string? keyPrefix = null)
    {
        this.KeyPrefix = keyPrefix;
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

    public async Task<DeleteObjectResponse> DeleteFile(RemoteFileDetail fileDetail)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = this.BucketName,
            Key = this.GetKey(fileDetail)
        };
        return await this.Client.DeleteObjectAsync(request);
    }

    public async Task<DeleteObjectsResponse> DeleteFolder()
    {
        if (string.IsNullOrWhiteSpace(this.KeyPrefix))
        {
            throw new InvalidOperationException("Folder deletion is only allowed in test contexts with a KeyPrefix set.");
        }
        var listRequest = new ListObjectsV2Request
        {
            BucketName = this.BucketName,
            Prefix = this.KeyPrefix + "/"
        };
        var keysToDelete = new List<KeyVersion>();
        ListObjectsV2Response listResponse;
        do
        {
            listResponse = await this.Client.ListObjectsV2Async(listRequest);
            foreach (S3Object obj in listResponse.S3Objects)
            {
                if (keysToDelete.Count < 1000)
                {
                    keysToDelete.Add(new KeyVersion { Key = obj.Key });
                }
                else
                {
                    // if there are more than 1000 keys, we'll get the rest on the next test run from this class
                    break;
                }
            }
            listRequest.ContinuationToken = listResponse.NextContinuationToken;
        } while (listResponse.IsTruncated);
        var request = new DeleteObjectsRequest
        {
            BucketName = this.BucketName,
            Objects = keysToDelete
        };
        return await this.Client.DeleteObjectsAsync(request);
    }

    public async Task<GetObjectResponse> GetFile(RemoteFileDetail fileDetail)
    {
        var request = new GetObjectRequest
        {
            BucketName = this.BucketName,
            Key = this.GetKey(fileDetail)
        };
        return await this.Client.GetObjectAsync(request);
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

    public async Task<CopyObjectResponse> UpdateFileContentType(RemoteFileDetail fileDetail, string contentType)
    {
        var key = this.GetKey(fileDetail);
        var request = new CopyObjectRequest
        {
            SourceBucket = this.BucketName,
            SourceKey = key,
            DestinationBucket = this.BucketName,
            DestinationKey = key,
            ContentType = contentType,
            MetadataDirective = S3MetadataDirective.REPLACE
        };
        return await this.Client.CopyObjectAsync(request);
    }
}
