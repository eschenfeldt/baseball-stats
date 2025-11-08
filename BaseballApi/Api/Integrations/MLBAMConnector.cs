using System;

namespace BaseballApi.Integrations;

public class MLBAMConnector
{
    public const string BaseUrl = "https://statsapi.mlb.com/api/v1/";

    public async Task<MlbPeopleResponse> GetPeopleAsync(DateOnly updatedSince, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}people/changes?updatedSince={updatedSince:yyyy-MM-dd}";
        using var httpClient = new HttpClient();
        return await httpClient.GetFromJsonAsync<MlbPeopleResponse>(url, cancellationToken);
    }
}
