namespace BaseballApi.Integrations;

public class MLBAMConnector(HttpClient httpClient) : IMLBAMConnector
{
    public async Task<MLBAMPeopleResponse> GetPeopleAsync(DateOnly updatedSince, CancellationToken cancellationToken = default)
    {
        var url = $"people/changes?updatedSince={updatedSince:yyyy-MM-dd}";
        return await httpClient.GetFromJsonAsync<MLBAMPeopleResponse>(url, cancellationToken);
    }

    public async Task<MLBAMPlayersResponse> GetPlayersAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<MLBAMPlayersResponse>("sports/1/players", cancellationToken);
    }

    public async Task<MLBAMTeamsResponse> GetTeamsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<MLBAMTeamsResponse>("teams", cancellationToken);
    }
}
