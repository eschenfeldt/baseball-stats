using System;
using BaseballApi.Contracts;
using BaseballApi.Controllers;
using BaseballApi.Models;
using BaseballApi.Services;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BaseballApiTests;

public class TestMediaImporter(BaseballContext context, MediaController controller, MediaImportBackgroundService backgroundService)
{
    private BaseballContext Context { get; } = context;
    private MediaController Controller { get; } = controller;
    private MediaImportBackgroundService MediaImportBackgroundService { get; } = backgroundService;

    internal async Task<ImportTask> ImportMedia(List<IFormFile> files, long gameId, Dictionary<string, MediaResourceType> resourceTypes)
    {
        // start up the media import background service
        await MediaImportBackgroundService.StartAsync(CancellationToken.None);

        var startTime = DateTimeOffset.Now;
        var importTask = await Controller.ImportMedia(files, JsonConvert.SerializeObject(gameId));
        var initializedTime = DateTimeOffset.Now;
        var initializationTime = initializedTime - startTime;
        Assert.InRange(initializationTime.TotalSeconds, 0, 5);
        Assert.NotNull(importTask);
        Assert.Equal(MediaImportTaskStatus.Queued, importTask.Value.Status);
        Assert.Equal(0, importTask.Value.Progress);

        var photoCount = resourceTypes.Values.Count(r => r == MediaResourceType.Photo);
        var videoCount = resourceTypes.Values.Count(r => r == MediaResourceType.Video);
        var livePhotoCount = resourceTypes.Values.Count(r => r == MediaResourceType.LivePhoto) / 2; // each live photo consists of two files
        string expectedMessage = $"Importing {Pluralize(photoCount, "photo")}, {Pluralize(videoCount, "video")}, and {Pluralize(livePhotoCount, "live photo")}";
        Assert.Equal(expectedMessage, importTask.Value.Message);

        ValidateGameData(gameId, importTask.Value.Id);

        int i = 0;
        DateTimeOffset lastUpdateTime;
        do
        {
            lastUpdateTime = DateTimeOffset.Now;
            await Task.Delay(1000);
            // force a refresh of the status since the background service updates it asynchronously in the db via a different context
            var importDbEntry = await Context.MediaImportTasks.FirstOrDefaultAsync(x => x.Id == importTask.Value.Id);
            if (importDbEntry != null)
            {
                Context.Entry(importDbEntry).Reload();
            }
            importTask = await Controller.GetImportStatus(importTask.Value.Id);
            Assert.NotNull(importTask);
            if (importTask.Value.Status == MediaImportTaskStatus.Queued)
            {
                Assert.True(i < 10, "Import task is still queued after several checks, which is unexpected.");
                ValidateGameData(gameId, importTask.Value.Id);
            }
            else if (importTask.Value.Status == MediaImportTaskStatus.Failed)
            {
                Assert.Fail($"Import task failed with message: {importTask.Value.Message}");
            }
            else if (importTask.Value.Status == MediaImportTaskStatus.Completed)
            {
                Assert.Equal(MediaImportTaskStatus.Completed, importTask.Value.Status);
                Assert.Equal($"Imported {Pluralize(photoCount, "photo")}, {Pluralize(videoCount, "video")}, and {Pluralize(livePhotoCount, "live photo")}", importTask.Value.Message);
                break;
            }
            else
            {
                Assert.Equal(MediaImportTaskStatus.InProgress, importTask.Value.Status);
                Assert.InRange(importTask.Value.Progress, 0, 1);
                ValidateGameData(gameId, importTask.Value.Id);
                Assert.NotNull(importTask.Value.StartTime);
                DateTimeOffset actualStartTime = importTask.Value.StartTime.Value;
                Assert.InRange(actualStartTime, startTime, DateTimeOffset.Now);
            }
            Assert.Equal(expectedMessage, importTask.Value.Message);
            i++;
        } while (i < 300);

        Assert.Equal(MediaImportTaskStatus.Completed, importTask.Value.Status);
        expectedMessage = $"Imported {Pluralize(photoCount, "photo")}, {Pluralize(videoCount, "video")}, and {Pluralize(livePhotoCount, "live photo")}";
        Assert.Equal(expectedMessage, importTask.Value.Message);
        Assert.NotNull(importTask.Value.EndTime);
        DateTimeOffset actualEndTime = importTask.Value.EndTime.Value;
        Assert.InRange(actualEndTime, lastUpdateTime, DateTimeOffset.Now);

        // stop the service again
        await MediaImportBackgroundService.StopAsync(CancellationToken.None);

        return importTask.Value;
    }

    private async void ValidateGameData(long gameId, Guid expectedTaskId)
    {
        var taskFromGame = await Controller.GetImportTasks(gameId: gameId);
        Assert.NotNull(taskFromGame);
        Assert.NotNull(taskFromGame.Value);
        Assert.Single(taskFromGame.Value);
        Assert.Equal(expectedTaskId, taskFromGame.Value[0].Id);
    }

    private static string Pluralize(int count, string singular)
    {
        return count == 1 ? $"{count} {singular}" : $"{count} {singular.Pluralize()}";
    }
}
