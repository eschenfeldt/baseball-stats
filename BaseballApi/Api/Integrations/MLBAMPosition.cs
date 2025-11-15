using System.Text.Json.Serialization;

namespace BaseballApi.Integrations;

public readonly record struct MLBAMPosition(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("abbreviation")] string Abbreviation
);