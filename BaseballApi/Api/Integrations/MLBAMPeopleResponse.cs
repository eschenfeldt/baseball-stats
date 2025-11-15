using System.Text.Json.Serialization;

namespace BaseballApi.Integrations;

public readonly record struct MLBAMPeopleResponse(
    [property: JsonPropertyName("copyright")] string Copyright,
    [property: JsonPropertyName("people")] IReadOnlyList<MLBAMPerson> People
);
