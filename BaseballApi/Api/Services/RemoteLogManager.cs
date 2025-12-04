using System;
using Amazon.S3;
using Amazon.S3.Model;

namespace BaseballApi.Services;

public class RemoteLogManager : IRemoteLogManager
{
    private ILogger<RemoteLogManager> Logger { get; }
    private string LogDirectory { get; }
    private string BucketName { get; }
    private string? KeyPrefix { get; }
    private AmazonS3Client Client { get; }
    private int RetainDays { get; }

    private string GetKey(string fileName)
    {
        var key = "Logs/" + fileName;
        if (!string.IsNullOrWhiteSpace(this.KeyPrefix))
        {
            key = this.KeyPrefix + "/" + key;
        }
        return key;
    }

    public RemoteLogManager(ILogger<RemoteLogManager> logger, IConfiguration configuration, string? keyPrefix = null)
    {
        Logger = logger;
        KeyPrefix = keyPrefix ?? configuration["Logging:File:RemoteKeyPrefix"];
        LogDirectory = configuration["Logging:File:Directory"] ?? "logs";
        RetainDays = int.Parse(configuration["Logging:File:RetainDays"] ?? "30");
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
        this.BucketName = configuration["Spaces:Bucket"] ?? "eschenfeldt-baseball-logs";
    }

    public async Task UploadPendingLogs(CancellationToken cancellationToken, bool allowInProgress = false)
    {
        var logFiles = Directory.GetFiles(LogDirectory, "*.log");
        List<string> toUpload;
        if (allowInProgress)
        {
            toUpload = [.. logFiles];
        }
        else
        {
            toUpload = [.. logFiles.Where(f => !Path.GetFileName(f).StartsWith(DateTime.UtcNow.ToString("yyyy_MM_dd")))];
        }
        if (toUpload.Count == 0)
        {
            Logger.LogInformation("No completed log files to upload.");
            return;
        }
        Logger.LogInformation("Uploading {count} completed log files.", toUpload.Count);
        foreach (var logFile in toUpload)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Logger.LogInformation("Cancellation requested, stopping log upload.");
                break;
            }
            try
            {
                var key = GetKey(Path.GetFileName(logFile));

                using FileStream fileStream = File.OpenRead(logFile);
                PutObjectRequest request = new()
                {
                    BucketName = BucketName,
                    Key = key,
                    InputStream = fileStream,
                    CannedACL = S3CannedACL.Private
                };
                await Client.PutObjectAsync(request, cancellationToken);

                Logger.LogInformation("Uploaded log file {logFile} to remote storage.", logFile);
                File.Delete(logFile);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error uploading log file {logFile}.", logFile);
            }
        }
    }

    public async Task CleanupOldLogs(CancellationToken cancellationToken, int? retainDays = null)
    {
        try
        {
            var listRequest = new ListObjectsV2Request
            {
                BucketName = BucketName,
                Prefix = GetKey("")
            };

            ListObjectsV2Response listResponse;
            do
            {
                listResponse = await Client.ListObjectsV2Async(listRequest, cancellationToken);
                var cutoffDays = retainDays ?? RetainDays;
                var oldObjects = listResponse.S3Objects
                    .Where(o => o.LastModified < DateTime.UtcNow.AddDays(-1 * cutoffDays))
                    .ToList();

                if (oldObjects.Count > 0)
                {
                    Logger.LogInformation("Deleting {count} old log files from remote storage.", oldObjects.Count);
                    var deleteRequest = new DeleteObjectsRequest
                    {
                        BucketName = BucketName,
                        Objects = [.. oldObjects.Select(o => new KeyVersion { Key = o.Key })]
                    };
                    await Client.DeleteObjectsAsync(deleteRequest, cancellationToken);
                }

                listRequest.ContinuationToken = listResponse.NextContinuationToken;
            } while (listResponse.IsTruncated && !cancellationToken.IsCancellationRequested);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error cleaning up old log files.");
        }
    }

    public async Task<List<string>> GetUploadedLogFiles()
    {
        var uploadedFiles = new List<string>();
        var listRequest = new ListObjectsV2Request
        {
            BucketName = BucketName,
            Prefix = GetKey("")
        };

        ListObjectsV2Response listResponse;
        do
        {
            listResponse = await Client.ListObjectsV2Async(listRequest);
            uploadedFiles.AddRange(listResponse.S3Objects.Select(o => o.Key));

            listRequest.ContinuationToken = listResponse.NextContinuationToken;
        } while (listResponse.IsTruncated);

        return uploadedFiles;
    }
}
