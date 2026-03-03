namespace BaseballApi.Integrations;

using System.Text.Json.Serialization;

public readonly record struct MLBAMReference(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("link")] string Link
);
