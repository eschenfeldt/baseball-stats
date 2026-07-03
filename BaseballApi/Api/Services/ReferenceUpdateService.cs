using BaseballApi.Integrations;
using BaseballApi.Observability;

namespace BaseballApi.Services;

public class ReferenceUpdateService(
        IServiceProvider serviceProvider,
        ILogger<ReferenceUpdateService> logger) : BackgroundService
{
    private IServiceProvider ServiceProvider { get; } = serviceProvider;
    private ILogger<ReferenceUpdateService> Logger { get; } = logger;
    private CancellationToken CancellationToken { get; set; }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Reference data update service started.");
        CancellationToken = cancellationToken;
        var teamsTimer = new Timer(UpdateTeamReferences, null, TimeSpan.Zero, TimeSpan.FromHours(12));
        var playerTimer = new Timer(UpdatePlayerReferences, null, TimeSpan.FromMinutes(30), TimeSpan.FromHours(24));
        var fangraphsTimer = new Timer(UpdateFangraphsLinks, null, TimeSpan.FromMinutes(60), TimeSpan.FromHours(6));
        try
        {
            await Task.Delay(Timeout.Infinite, CancellationToken);
        }
        finally
        {
            teamsTimer.Dispose();
            playerTimer.Dispose();
            fangraphsTimer.Dispose();
            Logger.LogInformation("Reference data update service stopped.");
        }
    }

    private async void UpdateTeamReferences(object? stateInfo)
    {
        using var activity = Telemetry.StartJob("job.update-team-references");
        try
        {
            using var scope = ServiceProvider.CreateScope();
            var referenceManager = scope.ServiceProvider.GetRequiredService<ReferenceManager>();
            var updatedCount = await referenceManager.UpdateTeamReferences(CancellationToken);
            Logger.LogInformation("Team reference update completed. {updatedCount} teams updated.", updatedCount);
        }
        catch (Exception ex)
        {
            Telemetry.RecordJobException(activity, ex);
            Logger.LogError(ex, "Error occurred while updating team references.");
        }
    }

    private async void UpdatePlayerReferences(object? stateInfo)
    {
        using var activity = Telemetry.StartJob("job.update-player-references");
        try
        {
            using var scope = ServiceProvider.CreateScope();
            var referenceManager = scope.ServiceProvider.GetRequiredService<ReferenceManager>();
            var result = await referenceManager.UpdatePlayerReferences(CancellationToken);
            Logger.LogInformation("Player reference update completed. {createdCount} players created, {updatedCount} players updated.",
                result.CreatedCount, result.UpdatedCount);
        }
        catch (Exception ex)
        {
            Telemetry.RecordJobException(activity, ex);
            Logger.LogError(ex, "Error occurred while updating player references.");
        }
    }

    private async void UpdateFangraphsLinks(object? stateInfo)
    {
        using var activity = Telemetry.StartJob("job.update-fangraphs-links");
        try
        {
            using var scope = ServiceProvider.CreateScope();
            var referenceManager = scope.ServiceProvider.GetRequiredService<ReferenceManager>();
            var result = await referenceManager.UpdateFangraphsLinks(CancellationToken);
            Logger.LogInformation("Fangraphs link update completed. {playerUpdateCount} players updated, {referencePlayerUpdateCount} reference players updated.",
                result.PlayersUpdated, result.ReferencePlayersUpdated);
        }
        catch (Exception ex)
        {
            Telemetry.RecordJobException(activity, ex);
            Logger.LogError(ex, "Error occurred while updating Fangraphs links.");
        }
    }
}
