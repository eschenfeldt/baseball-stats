
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BaseballApi.Integrations;

public readonly record struct MLBAMPlayer(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("fullName")] string FullName,
    [property: JsonPropertyName("link")] string Link,
    [property: JsonPropertyName("firstName")] string? FirstName,
    [property: JsonPropertyName("lastName")] string? LastName,
    [property: JsonPropertyName("primaryNumber")] string? PrimaryNumber,
    [property: JsonPropertyName("birthDate")] string? BirthDate,
    [property: JsonPropertyName("currentAge")] int? CurrentAge,
    [property: JsonPropertyName("birthCity")] string? BirthCity,
    [property: JsonPropertyName("birthStateProvince")] string? BirthStateProvince,
    [property: JsonPropertyName("birthCountry")] string? BirthCountry,
    [property: JsonPropertyName("height")] string? Height,
    [property: JsonPropertyName("weight")] int? Weight,
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("currentTeam")] MLBAMReference CurrentTeam,
    [property: JsonPropertyName("primaryPosition")] MLBAMPosition PrimaryPosition,
    [property: JsonPropertyName("useName")] string? UseName,
    [property: JsonPropertyName("useLastName")] string? UseLastName,
    [property: JsonPropertyName("middleName")] string? MiddleName,
    [property: JsonPropertyName("boxscoreName")] string? BoxscoreName,
    [property: JsonPropertyName("nickName")] string? NickName,
    [property: JsonPropertyName("gender")] string? Gender,
    [property: JsonPropertyName("nameMatrilineal")] string? NameMatrilineal,
    [property: JsonPropertyName("isPlayer")] bool IsPlayer,
    [property: JsonPropertyName("isVerified")] bool IsVerified,
    [property: JsonPropertyName("draftYear")] int? DraftYear,
    [property: JsonPropertyName("pronunciation")] string? Pronunciation,
    [property: JsonPropertyName("mlbDebutDate")] string? MlbDebutDate,
    [property: JsonPropertyName("batSide")] MLBAMHand BatSide,
    [property: JsonPropertyName("pitchHand")] MLBAMHand PitchHand,
    [property: JsonPropertyName("nameFirstLast")] string? NameFirstLast,
    [property: JsonPropertyName("nameSlug")] string? NameSlug,
    [property: JsonPropertyName("firstLastName")] string? FirstLastName,
    [property: JsonPropertyName("lastFirstName")] string? LastFirstName,
    [property: JsonPropertyName("lastInitName")] string? LastInitName,
    [property: JsonPropertyName("initLastName")] string? InitLastName,
    [property: JsonPropertyName("fullFMLName")] string? FullFmlName,
    [property: JsonPropertyName("fullLFMName")] string? FullLfmName,
    [property: JsonPropertyName("strikeZoneTop")] double? StrikeZoneTop,
    [property: JsonPropertyName("strikeZoneBottom")] double? StrikeZoneBottom
);






