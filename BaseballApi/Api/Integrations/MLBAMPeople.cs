using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BaseballApi.Integrations;

public readonly record struct MlbPeopleResponse(
    [property: JsonPropertyName("copyright")] string Copyright,
    [property: JsonPropertyName("people")] IReadOnlyList<MlbPerson> People
);

public readonly record struct MlbPerson(
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
    [property: JsonPropertyName("primaryPosition")] MlbPosition PrimaryPosition,
    [property: JsonPropertyName("useName")] string? UseName,
    [property: JsonPropertyName("useLastName")] string? UseLastName,
    [property: JsonPropertyName("middleName")] string? MiddleName,
    [property: JsonPropertyName("boxscoreName")] string? BoxscoreName,
    [property: JsonPropertyName("nickName")] string? NickName,
    [property: JsonPropertyName("gender")] string? Gender,
    [property: JsonPropertyName("isPlayer")] bool IsPlayer,
    [property: JsonPropertyName("isVerified")] bool IsVerified,
    [property: JsonPropertyName("draftYear")] int? DraftYear,
    [property: JsonPropertyName("pronunciation")] string? Pronunciation,
    [property: JsonPropertyName("mlbDebutDate")] string? MlbDebutDate,
    [property: JsonPropertyName("batSide")] MlbHand BatSide,
    [property: JsonPropertyName("pitchHand")] MlbHand PitchHand,
    [property: JsonPropertyName("nameFirstLast")] string? NameFirstLast,
    [property: JsonPropertyName("nameSlug")] string? NameSlug,
    [property: JsonPropertyName("strikeZoneTop")] double? StrikeZoneTop,
    [property: JsonPropertyName("strikeZoneBottom")] double? StrikeZoneBottom
);

public readonly record struct MlbPosition(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("abbreviation")] string Abbreviation
);

public readonly record struct MlbHand(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("description")] string Description
);
