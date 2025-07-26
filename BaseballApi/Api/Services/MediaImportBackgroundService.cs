using BaseballApi.Import;
using BaseballApi.Media;
using BaseballApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Services;

public class MediaImportBackgroundService(
        IMediaImportQueue mediaImportQueue,
        IServiceProvider serviceProvider,
        ILogger<MediaImportBackgroundService> logger) : BackgroundService
{
    private IMediaImportQueue MediaImportQueue { get; } = mediaImportQueue;
    private IServiceProvider ServiceProvider { get; } = serviceProvider;
    private ILogger<MediaImportBackgroundService> Logger { get; } = logger;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Wait for an import task to be available
                var importId = await MediaImportQueue.PopAsync(cancellationToken);
                Logger.LogInformation("Processing import task with ID: {ImportId}", importId);

                // Create a scope to resolve services
                using var scope = ServiceProvider.CreateScope();
                var remoteFileManager = scope.ServiceProvider.GetRequiredService<IRemoteFileManager>();
                var context = scope.ServiceProvider.GetRequiredService<BaseballContext>();

                try
                {
                    MediaImportQueue.MarkImportInProgress();
                    // Process the import task
                    await ProcessImport(importId, remoteFileManager, context, cancellationToken);
                }
                finally
                {
                    MediaImportQueue.MarkImportComplete();
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("Media Import Background Service was cancelled.");
                break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while processing the media import task.");
            }
        }
    }

    private async Task ProcessImport(Guid importId, IRemoteFileManager remoteFileManager, BaseballContext context, CancellationToken cancellationToken)
    {
        // Retrieve the import task from the database
        var importTask = await context.MediaImportTasks
            .Include(t => t.Game)
            .Include(t => t.MediaToProcess)
            .SingleOrDefaultAsync(t => t.Id == importId, cancellationToken: cancellationToken);
        if (importTask == null)
        {
            Logger.LogWarning("Import task with ID {ImportId} not found.", importId);
            return;
        }

        if (importTask.Status != MediaImportTaskStatus.Queued &&
            importTask.Status != MediaImportTaskStatus.InProgress)
        {
            Logger.LogWarning("Import task with ID {ImportId} is not in a valid state for processing: {Status}", importId, importTask.Status);
            return;
        }

        // Update the status to InProgress
        importTask.StartedAt = DateTimeOffset.UtcNow;
        importTask.Status = MediaImportTaskStatus.InProgress;
        await context.SaveChangesAsync(cancellationToken);

        // Get the list of files and associated game
        var game = importTask.Game;

        // Process the media files
        var importManager = new MediaImportManager([.. importTask.MediaToProcess], remoteFileManager, context, importTask.Game);
        Logger.LogInformation("Starting import for {Count} media resources for import task with ID {ImportId}.", importTask.MediaToProcess.Count, importId);
        int errorCount = 0;
        await foreach (var result in importManager.GetUploadedResources())
        {
            if (result.Resource != null)
            {
                result.Resource.Game = game;
                game?.Media.Add(result.Resource);
                context.MediaResources.Add(result.Resource);
            }
            else
            {
                Logger.LogWarning("Failed to process {resource} for import task: {message}.", result.OriginalResource, result.ErrorMessage);
                errorCount++;
            }
            // Save incrementally so status updates with each resource processed
            await context.SaveChangesAsync(cancellationToken);
        }

        if (errorCount == 0)
        {
            importTask.Status = MediaImportTaskStatus.Completed;
        }
        else
        {
            importTask.Status = MediaImportTaskStatus.Failed;
        }
        importTask.CompletedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Media import task with ID {ImportId} completed.", importId);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Stopping Media Import Background Service.");
        await base.StopAsync(cancellationToken);
    }
}
