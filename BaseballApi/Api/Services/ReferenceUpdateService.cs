using System;
using BaseballApi.Integrations;

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
        var teamsTimer = new Timer(UpdateTeamReferences, null, TimeSpan.Zero, TimeSpan.FromHours(12));
        var playerTimer = new Timer(UpdatePlayerReferences, null, TimeSpan.FromMinutes(30), TimeSpan.FromHours(24));
        CancellationToken = cancellationToken;
        // Wait for the timers to trigger
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        teamsTimer.Dispose();
        playerTimer.Dispose();
        Logger.LogInformation("Reference data update service stopped.");
    }

    private async void UpdateTeamReferences(object? stateInfo)
    {
        throw new NotImplementedException();
    }

    private async void UpdatePlayerReferences(object? stateInfo)
    {
        throw new NotImplementedException();
    }
}
