using System.Text.Json.Serialization;

namespace BaseballApi.Integrations;

public readonly record struct MLBAMPlayersResponse(
    [property: JsonPropertyName("copyright")] string Copyright,
    [property: JsonPropertyName("people")] IReadOnlyList<MLBAMPlayer> People
);
