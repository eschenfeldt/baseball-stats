using System;
using BaseballApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Services;

public class MediaImportTaskRestarter(IMediaImportQueue mediaImportQueue, IServiceProvider serviceProvider, ILogger<MediaImportBackgroundService> logger) : BackgroundService
{
    private IMediaImportQueue MediaImportQueue { get; } = mediaImportQueue;
    private IServiceProvider ServiceProvider { get; } = serviceProvider;
    private ILogger<MediaImportBackgroundService> Logger { get; } = logger;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Media Import Task Restarter started.");
        var timer = new Timer(RetriggerImports, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        // Wait for the timer to trigger
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        timer.Dispose();
        Logger.LogInformation("Media Import Task Restarter stopped.");
    }

    private async void RetriggerImports(object? stateInfo)
    {
        try
        {
            Logger.LogInformation("Retriggering abandoned media import tasks...");
            using var scope = ServiceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<BaseballContext>();

            // Find all import tasks that are in a non-complete state
            var possiblyAbandonedTasks = await context.MediaImportTasks
                .Where(t => t.Status == MediaImportTaskStatus.InProgress ||
                            t.Status == MediaImportTaskStatus.Queued)
                .ToListAsync();

            foreach (var task in possiblyAbandonedTasks)
            {
                // Re-queue the task for processing
                await MediaImportQueue.PushAsync(task.Id);
                Logger.LogInformation("Re-queued import task with ID: {ImportId}", task.Id);
                // if it was actually still in the queue it will just log a warning when it tries to reprocess
            }
            Logger.LogInformation("Retriggering of abandoned media import tasks completed.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while retriggering imports.");
        }
    }
}
