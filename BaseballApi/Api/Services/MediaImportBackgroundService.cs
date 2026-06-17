using BaseballApi.Import;
using BaseballApi.Media;
using BaseballApi.Models;
using BaseballApi.Observability;
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

                using var activity = Telemetry.BackgroundJobs.StartActivity("job.media-import");
                activity?.SetTag("import.id", importId);

                using var logScope = Logger.BeginScope(new Dictionary<string, object>
                {
                    ["ImportId"] = importId,
                });
                Logger.LogInformation("Processing import task.");

                try
                {
                    // Create a scope to resolve services
                    using var scope = ServiceProvider.CreateScope();
                    var remoteFileManager = scope.ServiceProvider.GetRequiredService<IRemoteFileManager>();
                    using var context = scope.ServiceProvider.GetRequiredService<BaseballContext>();

                    MediaImportQueue.MarkImportInProgress();
                    try
                    {
                        // Process the import task
                        await ProcessImport(importId, remoteFileManager, context, cancellationToken);
                    }
                    finally
                    {
                        MediaImportQueue.MarkImportComplete();
                    }
                }
                catch (Exception ex)
                {
                    // Records non-cancellation failures on the span; rethrows so the outer
                    // handler still logs (and a cancellation still breaks the loop).
                    Telemetry.RecordJobException(activity, ex);
                    throw;
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
            Logger.LogWarning("Import task not found.");
            Telemetry.RecordMediaImportOutcome(Telemetry.MediaImportOutcome.Skipped);
            return;
        }

        if (importTask.Status != MediaImportTaskStatus.Queued &&
            importTask.Status != MediaImportTaskStatus.InProgress)
        {
            Logger.LogWarning("Import task is not in a valid state for processing: {Status}", importTask.Status);
            Telemetry.RecordMediaImportOutcome(Telemetry.MediaImportOutcome.Skipped);
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
        Logger.LogInformation("Starting import for {Count} media resources.", importTask.MediaToProcess.Count);
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
            Telemetry.RecordMediaImportOutcome(Telemetry.MediaImportOutcome.Completed);
        }
        else
        {
            importTask.Status = MediaImportTaskStatus.Failed;
            Telemetry.RecordMediaImportOutcome(Telemetry.MediaImportOutcome.Failed);
        }
        importTask.CompletedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Media import task completed.");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Stopping Media Import Background Service.");
        await base.StopAsync(cancellationToken);
    }
}
