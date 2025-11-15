using System.Text.Json.Serialization;

namespace BaseballApi.Integrations;

public readonly record struct MLBAMHand(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("description")] string Description
);
