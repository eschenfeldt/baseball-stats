using System.Text.Json.Serialization;

namespace BaseballApi.Integrations;

public readonly record struct FangraphsPlayer(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("namekorean")] string? NameKorean,
    [property: JsonPropertyName("lastname")] string LastName,
    [property: JsonPropertyName("firstname")] string FirstName,
    [property: JsonPropertyName("birthdate")] DateTime? BirthDate,
    [property: JsonPropertyName("team")] string Team,
    [property: JsonPropertyName("views")] int? Views,
    [property: JsonPropertyName("war")] double? War,
    [property: JsonPropertyName("position")] string Position,
    [property: JsonPropertyName("debut_season")] string? DebutSeason,
    [property: JsonPropertyName("last_season")] string? LastSeason,
    [property: JsonPropertyName("international")] int International,
    [property: JsonPropertyName("level")] IReadOnlyList<string> Level,
    [property: JsonPropertyName("teamid")] int TeamId,
    [property: JsonPropertyName("abbname")] string AbbName,
    [property: JsonPropertyName("url")] string Url
);
