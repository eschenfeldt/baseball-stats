using System;
using BaseballApi.Integrations;
using Newtonsoft.Json;

namespace BaseballApiTests.Mocks;

public class MockMLBAMConnector : IMLBAMConnector
{
    public Task<MLBAMPeopleResponse> GetPeopleAsync(DateOnly updatedSince, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<MLBAMPlayersResponse> GetPlayersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new MLBAMPlayersResponse
        {
            People = [
                JsonConvert.DeserializeObject<MLBAMPlayer>(willSmithCatcherJson),
                JsonConvert.DeserializeObject<MLBAMPlayer>(shoheiOhtaniJson),
                JsonConvert.DeserializeObject<MLBAMPlayer>(shotaImanagaJson)
            ]
        });
    }

    public Task<MLBAMTeamsResponse> GetTeamsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private static readonly string willSmithCatcherJson = """
    {
    "id": 669257,
    "fullName": "Will Smith",
    "link": "/api/v1/people/669257",
    "firstName": "William",
    "lastName": "Smith",
    "primaryNumber": "16",
    "birthDate": "1995-03-28",
    "currentAge": 30,
    "birthCity": "Louisville",
    "birthStateProvince": "KY",
    "birthCountry": "USA",
    "height": "5' 10\"",
    "weight": 195,
    "active": true,
    "currentTeam": {
        "id": 119,
        "link": "/api/v1/teams/119"
    },
    "primaryPosition": {
        "code": "2",
        "name": "Catcher",
        "type": "Catcher",
        "abbreviation": "C"
    },
    "useName": "Will",
    "useLastName": "Smith",
    "middleName": "Dills",
    "boxscoreName": "Smith, W",
    "nickName": "Smitty",
    "gender": "M",
    "isPlayer": true,
    "isVerified": false,
    "draftYear": 2016,
    "mlbDebutDate": "2019-05-28",
    "batSide": {
        "code": "R",
        "description": "Right"
    },
    "pitchHand": {
        "code": "R",
        "description": "Right"
    },
    "nameFirstLast": "Will Smith",
    "nameSlug": "will-smith-669257",
    "firstLastName": "Will Smith",
    "lastFirstName": "Smith, Will",
    "lastInitName": "Smith, W",
    "initLastName": "W Smith",
    "fullFMLName": "William Dills Smith",
    "fullLFMName": "Smith, William Dills",
    "strikeZoneTop": 3.39,
    "strikeZoneBottom": 1.6
}
""";
    private static readonly string shoheiOhtaniJson = """
    {
    "id": 660271,
    "fullName": "Shohei Ohtani",
    "link": "/api/v1/people/660271",
    "firstName": "Shohei",
    "lastName": "Ohtani",
    "primaryNumber": "17",
    "birthDate": "1994-07-05",
    "currentAge": 31,
    "birthCity": "Oshu",
    "birthCountry": "Japan",
    "height": "6' 3\"",
    "weight": 210,
    "active": true,
    "currentTeam": {
        "id": 119,
        "link": "/api/v1/teams/119"
    },
    "primaryPosition": {
        "code": "Y",
        "name": "Two-Way Player",
        "type": "Two-Way Player",
        "abbreviation": "TWP"
    },
    "useName": "Shohei",
    "useLastName": "Ohtani",
    "boxscoreName": "Ohtani",
    "nickName": "Showtime",
    "gender": "M",
    "isPlayer": true,
    "isVerified": false,
    "pronunciation": "show-HEY oh-TAWN-ee",
    "mlbDebutDate": "2018-03-29",
    "batSide": {
        "code": "L",
        "description": "Left"
    },
    "pitchHand": {
        "code": "R",
        "description": "Right"
    },
    "nameFirstLast": "Shohei Ohtani",
    "nameSlug": "shohei-ohtani-660271",
    "firstLastName": "Shohei Ohtani",
    "lastFirstName": "Ohtani, Shohei",
    "lastInitName": "Ohtani, S",
    "initLastName": "S Ohtani",
    "fullFMLName": "Shohei Ohtani",
    "fullLFMName": "Ohtani, Shohei",
    "strikeZoneTop": 3.51,
    "strikeZoneBottom": 1.64
}
""";

    private static readonly string shotaImanagaJson = """
    {
    "id": 684007,
    "fullName": "Shota Imanaga",
    "link": "/api/v1/people/684007",
    "firstName": "Shota",
    "lastName": "Imanaga",
    "primaryNumber": "18",
    "birthDate": "1993-09-01",
    "currentAge": 32,
    "birthCity": "Kitakyushu",
    "birthCountry": "Japan",
    "height": "5' 10\"",
    "weight": 175,
    "active": true,
    "currentTeam": {
        "id": 112,
        "name": "Chicago Cubs",
        "link": "/api/v1/teams/112"
    },
    "primaryPosition": {
        "code": "1",
        "name": "Pitcher",
        "type": "Pitcher",
        "abbreviation": "P"
    },
    "useName": "Shota",
    "useLastName": "Imanaga",
    "boxscoreName": "Imanaga",
    "gender": "M",
    "isPlayer": true,
    "isVerified": true,
    "pronunciation": "SHOW-tah ee-mah-NAH-gah",
    "mlbDebutDate": "2024-04-01",
    "batSide": {
        "code": "L",
        "description": "Left"
    },
    "pitchHand": {
        "code": "L",
        "description": "Left"
    },
    "nameFirstLast": "Shota Imanaga",
    "nameSlug": "shota-imanaga-684007",
    "firstLastName": "Shota Imanaga",
    "lastFirstName": "Imanaga, Shota",
    "lastInitName": "Imanaga, S",
    "initLastName": "S Imanaga",
    "fullFMLName": "Shota  Imanaga",
    "fullLFMName": "Imanaga, Shota ",
    "strikeZoneTop": 3.301,
    "strikeZoneBottom": 1.504
}
""";
}
