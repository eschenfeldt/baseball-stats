using System;

namespace BaseballApi.Integrations;

public interface IMLBAMConnector
{
    public Task<MLBAMPeopleResponse> GetPeopleAsync(DateOnly updatedSince, CancellationToken cancellationToken = default);

    public Task<MLBAMPlayersResponse> GetPlayersAsync(CancellationToken cancellationToken = default);

    public Task<MLBAMTeamsResponse> GetTeamsAsync(CancellationToken cancellationToken = default);
}
