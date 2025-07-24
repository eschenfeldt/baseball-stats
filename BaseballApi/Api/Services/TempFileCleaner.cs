using System;
using BaseballApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Services;

public class TempFileCleaner(IServiceProvider serviceProvider, ILogger<MediaImportBackgroundService> logger) : BackgroundService
{
    private IServiceProvider ServiceProvider { get; } = serviceProvider;
    private ILogger<MediaImportBackgroundService> Logger { get; } = logger;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Temp file cleaner started.");
        var timer = new Timer(CleanseTempFiles, null, TimeSpan.Zero, TimeSpan.FromHours(12));
        var abandonedFilesTimer = new Timer(CleanseAbandonedFiles, null, TimeSpan.Zero, TimeSpan.FromHours(24));
        // Wait for the timers to trigger
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        timer.Dispose();
        abandonedFilesTimer.Dispose();
        Logger.LogInformation("Temp file cleaner stopped.");
    }

    private async void CleanseTempFiles(object? stateInfo)
    {
        try
        {
            Logger.LogInformation("Identifying temp files to clean up...");
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BaseballContext>();

            // Find all resources that are either processed or associated with a completed or failed import task
            var resourcesToCleanUp = await context.MediaImportTasks
                .SelectMany(t => t.MediaToProcess)
                .Where(m => !m.FilesDeleted
                            && (m.MediaImportTask != null && (m.MediaImportTask.Status == MediaImportTaskStatus.Completed ||
                                                              m.MediaImportTask.Status == MediaImportTaskStatus.Failed)
                                || m.Status == MediaImportTaskStatus.Completed))
                .ToListAsync();

            Logger.LogInformation("Found {Count} resources to clean up.", resourcesToCleanUp.Count);
            foreach (var resource in resourcesToCleanUp)
            {
                Logger.LogInformation("Cleaning up {resourceType}: {ResourceName}", resource.ResourceType, resource.BaseName);
                var photoStillExists = false;
                var videoStillExists = false;
                // Delete the files associated with the resource
                if (!string.IsNullOrEmpty(resource.PhotoFilePath))
                {
                    if (File.Exists(resource.PhotoFilePath))
                    {
                        try
                        {
                            File.Delete(resource.PhotoFilePath);
                            Logger.LogInformation("Deleted photo file: {PhotoFilePath}", resource.PhotoFilePath);
                        }
                        catch (Exception ex)
                        {
                            photoStillExists = true;
                            Logger.LogError(ex, "Failed to delete photo file: {PhotoFilePath}", resource.PhotoFilePath);
                        }
                    }
                    else
                    {
                        Logger.LogWarning("Photo file does not exist: {PhotoFilePath}", resource.PhotoFilePath);
                    }
                }
                if (!string.IsNullOrEmpty(resource.VideoFilePath))
                {
                    if (File.Exists(resource.VideoFilePath))
                    {
                        try
                        {
                            File.Delete(resource.VideoFilePath);
                            Logger.LogInformation("Deleted video file: {VideoFilePath}", resource.VideoFilePath);
                        }
                        catch (Exception ex)
                        {
                            videoStillExists = true;
                            Logger.LogError(ex, "Failed to delete video file: {VideoFilePath}", resource.VideoFilePath);
                        }
                    }
                    else
                    {
                        Logger.LogWarning("Video file does not exist: {VideoFilePath}", resource.VideoFilePath);
                    }
                }
                // Mark the files as deleted in the database if appropriate
                if (!photoStillExists && !videoStillExists)
                {
                    resource.FilesDeleted = true;
                    await context.SaveChangesAsync();
                    Logger.LogInformation("Marked resource {ResourceName} as deleted in the database.", resource.BaseName);
                }
            }
            Logger.LogInformation("Retriggering of abandoned media import tasks completed.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while retriggering imports.");
        }
    }

    private async void CleanseAbandonedFiles(object? stateInfo)
    {
        try
        {
            Logger.LogInformation("Identifying abandoned files to clean up...");
            var path = Path.GetTempPath();
            var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".jpeg") || f.EndsWith(".mp4") || f.EndsWith(".tmp")) // extensions created as alternate formats and thumbnails
                .Select(f => new FileInfo(f))
                .Where(f => f.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-1))
                .ToList();
            var context = ServiceProvider.GetRequiredService<BaseballContext>();

            Logger.LogInformation("Found {Count} abandoned files for potential clean up.", files.Count);
            foreach (var file in files)
            {
                try
                {
                    if (!file.Exists)
                    {
                        Logger.LogWarning("Abandoned file does not exist: {FileName}", file.FullName);
                    }
                    else
                    {
                        var isUsedForImportTask = await context.MediaImportTasks
                            .AnyAsync(t => t.MediaToProcess.Any(m => m.PhotoFilePath == file.FullName || m.VideoFilePath == file.FullName));
                        if (isUsedForImportTask)
                        {
                            Logger.LogInformation("File {FileName} is still in use by an import task, skipping deletion.", file.FullName);
                        }
                        else
                        {
                            Logger.LogInformation("Deleting abandoned file: {FileName}", file.FullName);
                            file.Delete();
                            Logger.LogInformation("Deleted abandoned file: {FileName}", file.FullName);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to delete abandoned file: {FileName}", file.FullName);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while cleaning up abandoned files.");
        }
    }
}
