using System.Text.Json.Serialization;

namespace BaseballApi.Integrations;

public readonly record struct MLBAMTeamsResponse(
    [property: JsonPropertyName("copyright")] string Copyright,
    [property: JsonPropertyName("teams")] List<MLBAMTeam> Teams
);
