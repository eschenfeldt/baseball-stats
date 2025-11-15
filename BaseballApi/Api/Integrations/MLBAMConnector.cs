namespace BaseballApi.Integrations;

public class MLBAMConnector : IMLBAMConnector
{
    private const string BaseUrl = "https://statsapi.mlb.com/api/v1/";

    public async Task<MLBAMPeopleResponse> GetPeopleAsync(DateOnly updatedSince, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}people/changes?updatedSince={updatedSince:yyyy-MM-dd}";
        using var httpClient = new HttpClient();
        return await httpClient.GetFromJsonAsync<MLBAMPeopleResponse>(url, cancellationToken);
    }

    public async Task<MLBAMPlayersResponse> GetPlayersAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}sports/1/players";
        using var httpClient = new HttpClient();
        return await httpClient.GetFromJsonAsync<MLBAMPlayersResponse>(url, cancellationToken);
    }

    public async Task<MLBAMTeamsResponse> GetTeamsAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}teams";
        using var httpClient = new HttpClient();
        return await httpClient.GetFromJsonAsync<MLBAMTeamsResponse>(url, cancellationToken);
    }
}
