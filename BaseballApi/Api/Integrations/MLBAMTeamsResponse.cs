using System.Text.Json.Serialization;
using BaseballApi.Models;

namespace BaseballApi.Integrations;

public readonly record struct MLBAMTeamsResponse(
    [property: JsonPropertyName("copyright")] string Copyright,
    [property: JsonPropertyName("teams")] List<MLBAMTeam> Teams
);
