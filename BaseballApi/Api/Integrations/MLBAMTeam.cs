using System.Text.Json.Serialization;

namespace BaseballApi.Integrations;

public readonly record struct MLBAMTeam(
    [property: JsonPropertyName("allStarStatus")] string AllStarStatus,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("link")] string Link,
    [property: JsonPropertyName("season")] int Season,
    [property: JsonPropertyName("venue")] MLBAMReference Venue,
    [property: JsonPropertyName("teamCode")] string TeamCode,
    [property: JsonPropertyName("fileCode")] string FileCode,
    [property: JsonPropertyName("abbreviation")] string Abbreviation,
    [property: JsonPropertyName("teamName")] string TeamName,
    [property: JsonPropertyName("locationName")] string LocationName,
    [property: JsonPropertyName("firstYearOfPlay")] string FirstYearOfPlay,
    [property: JsonPropertyName("league")] MLBAMReference League,
    [property: JsonPropertyName("division")] MLBAMReference? Division,
    [property: JsonPropertyName("sport")] MLBAMReference Sport,
    [property: JsonPropertyName("shortName")] string ShortName,
    [property: JsonPropertyName("parentOrgName")] string? ParentOrgName,
    [property: JsonPropertyName("parentOrgId")] int? ParentOrgId,
    [property: JsonPropertyName("franchiseName")] string FranchiseName,
    [property: JsonPropertyName("clubName")] string ClubName,
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("springLeague")] MLBAMReference? SpringLeague,
    [property: JsonPropertyName("springVenue")] MLBAMReference? SpringVenue
);