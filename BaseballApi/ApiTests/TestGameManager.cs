using BaseballApi;
using BaseballApi.Contracts;
using BaseballApi.Models;

namespace BaseballApiTests;

public class TestGameManager
{
    private Dictionary<int, Team> Teams { get; } = [];
    private Dictionary<int, Player> Batters { get; } = [];
    private Dictionary<int, Player> Pitchers { get; } = [];
    private Dictionary<int, Park> Parks { get; } = [];

    public TestGameManager(BaseballContext context)
    {
        Teams.Add(1, context.Teams.First(t => t.City == "Test City"));
        Teams.Add(2, context.Teams.First(t => t.City == "New Tester Town"));
        Teams.Add(3, context.Teams.First(t => t.City == "St. Test"));
        Batters.Add(1, context.Players.First(p => p.Name == "Test Batter 1"));
        Batters.Add(2, context.Players.First(p => p.Name == "Test Batter 2"));
        Batters.Add(3, context.Players.First(p => p.Name == "Test Batter 3"));
        Pitchers.Add(1, context.Players.First(p => p.Name == "Test Pitcher 1"));
        Pitchers.Add(2, context.Players.First(p => p.Name == "Test Pitcher 2"));
        Parks.Add(1, context.Parks.First(p => p.Name == "Test Stadium"));
        Parks.Add(2, context.Parks.First(p => p.Name == "Test Park"));
    }

    public long GetTeamId(int teamNumber)
    {
        if (Teams.TryGetValue(teamNumber, out Team? team))
        {
            return team.Id;
        }
        else
        {
            Assert.Fail($"No test team defined with number {teamNumber}");
            return 0;
        }
    }

    public long GetGameId(BaseballContext context, int gameNumber)
    {
        if (TestGames.TryGetValue(gameNumber, out GameInfo gameInfo))
        {
            var gameId = context.Games.FirstOrDefault(g => g.Name == gameInfo.Name)?.Id;
            if (gameId == null)
            {
                Assert.Fail($"Test game with number {gameNumber} not found in database");
                return 0;
            }
            else
            {
                return gameId.Value;
            }
        }
        else
        {
            Assert.Fail($"No test game defined with number {gameNumber}");
            return 0;
        }
    }

    public long GetPlayerId(int? batterNumber = null, int? pitcherNumber = null)
    {
        if (pitcherNumber.HasValue && Pitchers.TryGetValue(pitcherNumber.Value, out Player? pitcher))
        {
            return pitcher.Id;
        }
        else if (batterNumber.HasValue && Batters.TryGetValue(batterNumber.Value, out Player? batter))
        {
            return batter.Id;
        }
        else
        {
            Assert.Fail($"No test player defined with pitcher number {pitcherNumber} or batter number {batterNumber}");
            return 0;
        }
    }

    public void ValidateGameSummary(GameSummary gameSummary, int gameNumber)
    {
        if (!TestGames.TryGetValue(gameNumber, out GameInfo gameInfo))
        {
            Assert.Fail($"Could not find test game with number {gameNumber}");
        }
        Assert.Equal(gameInfo.Name, gameSummary.Name);

        var park = Parks[gameInfo.ParkNumber];
        Assert.NotNull(gameSummary.Location);
        Assert.Equal(park.Name, gameSummary.Location.Name);

        var home = Teams[gameInfo.Home.TeamNumber];
        var defaultHomeName = $"{home.City} {home.Name}";
        Assert.Equal(gameInfo.Home.TeamName ?? defaultHomeName, gameSummary.HomeTeamName);
        Assert.Equal(home.Name, gameSummary.Home.Name);
        Assert.Equal(home.City, gameSummary.Home.City);

        var away = Teams[gameInfo.Away.TeamNumber];
        var defaultAwayName = $"{away.City} {away.Name}";
        Assert.Equal(gameInfo.Away.TeamName ?? defaultAwayName, gameSummary.AwayTeamName);
        Assert.Equal(away.Name, gameSummary.Away.Name);
        Assert.Equal(away.City, gameSummary.Away.City);
    }

    public void ValidateGame(GameDetail game, int gameNumber)
    {
        if (!TestGames.TryGetValue(gameNumber, out GameInfo gameInfo))
        {
            Assert.Fail($"Could not find test game with number {gameNumber}");
        }
        Assert.Equal(gameInfo.Name, game.Name);

        var park = Parks[gameInfo.ParkNumber];
        Assert.NotNull(game.Location);
        Assert.Equal(park.Name, game.Location.Name);

        var home = Teams[gameInfo.Home.TeamNumber];
        Assert.Equal(gameInfo.Home.TeamName ?? DefaultName(home), game.HomeTeamName);
        Assert.Equal(home.Name, game.Home.Name);
        Assert.Equal(home.City, game.Home.City);

        var away = Teams[gameInfo.Away.TeamNumber];
        Assert.Equal(gameInfo.Away.TeamName ?? DefaultName(away), game.AwayTeamName);
        Assert.Equal(away.Name, game.Away.Name);
        Assert.Equal(away.City, game.Away.City);

        Assert.NotNull(game.HomeBoxScore);
        Assert.NotNull(game.AwayBoxScore);

        var homeBatterRuns = game.HomeBoxScore.Value.Batters.Select(b => b.Stats[Stat.Runs.Name]).Sum();
        var awayBatterRuns = game.AwayBoxScore.Value.Batters.Select(b => b.Stats[Stat.Runs.Name]).Sum();
        var homePitcherRuns = game.HomeBoxScore.Value.Pitchers.Select(p => p.Stats[Stat.Runs.Name]).Sum();
        var awayPitcherRuns = game.AwayBoxScore.Value.Pitchers.Select(p => p.Stats[Stat.Runs.Name]).Sum();

        Assert.Equal(game.HomeScore, homeBatterRuns);
        Assert.Equal(game.HomeScore, awayPitcherRuns);
        Assert.Equal(game.AwayScore, awayBatterRuns);
        Assert.Equal(game.AwayScore, homePitcherRuns);

        Assert.Equal(gameInfo.Home.Batters.Count, game.HomeBoxScore.Value.Batters.Count);
        Assert.All(gameInfo.Home.Batters, (batterInfo) =>
        {
            ValidateBatter(batterInfo, game.HomeBoxScore.Value.Batters);
        });

        Assert.Equal(gameInfo.Away.Batters.Count, game.AwayBoxScore.Value.Batters.Count);
        Assert.All(gameInfo.Away.Batters, (batterInfo) =>
        {
            ValidateBatter(batterInfo, game.AwayBoxScore.Value.Batters);
        });

        Assert.Equal(gameInfo.Home.Pitchers.Count, game.HomeBoxScore.Value.Pitchers.Count);
        Assert.All(gameInfo.Home.Pitchers, (pitcherInfo) =>
        {
            ValidatePitcher(pitcherInfo, game.HomeBoxScore.Value.Pitchers);
        });

        Assert.Equal(gameInfo.Away.Pitchers.Count, game.AwayBoxScore.Value.Pitchers.Count);
        Assert.All(gameInfo.Away.Pitchers, (pitcherInfo) =>
        {
            ValidatePitcher(pitcherInfo, game.AwayBoxScore.Value.Pitchers);
        });

        Assert.Equal(gameInfo.Home.Fielders.Count, game.HomeBoxScore.Value.Fielders.Count);
        Assert.All(gameInfo.Home.Fielders, (fielderInfo) =>
        {
            ValidateFielder(fielderInfo, game.HomeBoxScore.Value.Fielders);
        });

        Assert.Equal(gameInfo.Away.Fielders.Count, game.AwayBoxScore.Value.Fielders.Count);
        Assert.All(gameInfo.Away.Fielders, (fielderInfo) =>
        {
            ValidateFielder(fielderInfo, game.AwayBoxScore.Value.Fielders);
        });
    }

    void ValidateBatter(BatterInfo expected, ICollection<GameBatter> allActual)
    {
        var batterId = GetPlayerId(batterNumber: expected.BatterNumber);
        var actual = allActual.FirstOrDefault(b => b.Player.Id == batterId);
        Assert.NotNull(actual);
        Assert.Equal(expected.AtBats, actual.Stats[Stat.AtBats.Name]);
        Assert.Equal(expected.Hits, actual.Stats[Stat.Hits.Name]);
        Assert.Equal(expected.Homeruns, actual.Stats[Stat.Homeruns.Name]);
        Assert.Equal(expected.PlateAppearances, actual.Stats[Stat.PlateAppearances.Name]);
        Assert.Equal(expected.Runs, actual.Stats[Stat.Runs.Name]);
    }

    void ValidatePitcher(PitcherInfo expected, ICollection<GamePitcher> allActual)
    {
        var pitcherId = GetPlayerId(pitcherNumber: expected.PitcherNumber);
        var actual = allActual.FirstOrDefault(b => b.Player.Id == pitcherId);
        Assert.NotNull(actual);
        Assert.Equal(expected.EarnedRuns, actual.Stats[Stat.EarnedRuns.Name]);
        Assert.Equal(expected.Outs, actual.Stats[Stat.ThirdInningsPitched.Name]);
        Assert.Equal(expected.Runs, actual.Stats[Stat.Runs.Name]);
    }


    void ValidateFielder(FielderInfo expected, ICollection<GameFielder> allActual)
    {
        var pitcherId = GetPlayerId(batterNumber: expected.BatterNumber, pitcherNumber: expected.PitcherNumber);
        var actual = allActual.FirstOrDefault(b => b.Player.Id == pitcherId);
        Assert.NotNull(actual);
        Assert.Equal(expected.Assists, actual.Stats[Stat.Assists.Name]);
        Assert.Equal(expected.Errors, actual.Stats[Stat.Errors.Name]);
        Assert.Equal(expected.Putouts, actual.Stats[Stat.Putouts.Name]);
    }

    public void AddAllGames(BaseballContext context)
    {
        foreach (var (gameNumber, gameInfo) in TestGames)
        {
            var home = Teams[gameInfo.Home.TeamNumber];
            var away = Teams[gameInfo.Away.TeamNumber];
            var park = Parks[gameInfo.ParkNumber];
            var game = new Game
            {
                Date = gameInfo.Date,
                Name = gameInfo.Name,
                Location = park,
                Home = home,
                HomeScore = gameInfo.HomeScore,
                HomeTeamName = gameInfo.Home.TeamName ?? DefaultName(home),
                Away = away,
                AwayScore = gameInfo.AwayScore,
                AwayTeamName = gameInfo.Away.TeamName ?? DefaultName(away),
                BoxScores = []
            };
            if (game.HomeScore > game.AwayScore)
            {
                game.WinningTeam = home;
                game.LosingTeam = away;
            }
            else if (game.HomeScore < game.AwayScore)
            {
                game.WinningTeam = away;
                game.LosingTeam = home;
            }
            context.AddRange(
                game
            );
            context.SaveChanges();
            var homeBox = new BoxScore
            {
                Game = game,
                Team = home,
            };
            var awayBox = new BoxScore
            {
                Game = game,
                Team = away
            };
            game.HomeBoxScore = homeBox;
            game.AwayBoxScore = awayBox;
            context.SaveChanges();
            PopulateBoxScore(context, homeBox, gameInfo.Home);
            PopulateBoxScore(context, awayBox, gameInfo.Away);
        }
    }

    void PopulateBoxScore(BaseballContext context, BoxScore boxScore, BoxScoreInfo info)
    {
        foreach (var batterInfo in info.Batters)
        {
            boxScore.Batters.Add(
                new Batter
                {
                    BoxScore = boxScore,
                    Player = Batters[batterInfo.BatterNumber],
                    Games = 1,
                    PlateAppearances = batterInfo.PlateAppearances,
                    AtBats = batterInfo.AtBats,
                    Hits = batterInfo.Hits,
                    Homeruns = batterInfo.Homeruns,
                    Runs = batterInfo.Runs
                }
            );
        }
        var pitchers = info.Pitchers.Select(pitcherInfo => new Pitcher
        {
            BoxScore = boxScore,
            Player = Pitchers[pitcherInfo.PitcherNumber],
            Games = 1,
            ThirdInningsPitched = pitcherInfo.Outs,
            Runs = pitcherInfo.Runs,
            EarnedRuns = pitcherInfo.EarnedRuns
        });
        context.AddRange(pitchers);
        var fielders = info.Fielders.Select(fielderInfo =>
        {
            Player player;
            if (fielderInfo.BatterNumber.HasValue)
            {
                player = Batters[fielderInfo.BatterNumber.Value];
            }
            else if (fielderInfo.PitcherNumber.HasValue)
            {
                player = Pitchers[fielderInfo.PitcherNumber.Value];
            }
            else
            {
                Assert.Fail($"Fielder without a player identifier: {fielderInfo}");
                throw new InvalidOperationException();
            }
            return new Fielder
            {
                BoxScore = boxScore,
                Player = player,
                Games = 1,
                Putouts = fielderInfo.Putouts,
                Assists = fielderInfo.Assists,
                Errors = fielderInfo.Errors
            };
        });
        context.AddRange(fielders);

        context.SaveChanges();
    }

    private string DefaultName(Team team)
    {
        return $"{team.City} {team.Name}";
    }

    struct GameInfo
    {
        public DateOnly Date { get; set; }
        public required int ParkNumber { get; set; }
        public string Name { get; set; }
        public BoxScoreInfo Home { get; set; }
        public required int HomeScore { get; set; }
        public BoxScoreInfo Away { get; set; }
        public required int AwayScore { get; set; }
    }

    struct BoxScoreInfo
    {
        public int TeamNumber { get; set; }
        public string? TeamName { get; set; }
        public required List<PitcherInfo> Pitchers { get; set; }
        public required List<BatterInfo> Batters { get; set; }
        public required List<FielderInfo> Fielders { get; set; }
    }

    struct PitcherInfo
    {
        public int PitcherNumber { get; set; }
        public int Outs { get; set; }
        public int Runs { get; set; }
        public int EarnedRuns { get; set; }
    }

    struct BatterInfo
    {
        public int BatterNumber { get; set; }
        public int PlateAppearances { get; set; }
        public int AtBats { get; set; }
        public int Hits { get; set; }
        public int Homeruns { get; set; }
        public int Runs { get; set; }
    }

    struct FielderInfo
    {
        public int? BatterNumber { get; set; }
        public int? PitcherNumber { get; set; }
        public int Putouts { get; set; }
        public int Assists { get; set; }
        public int Errors { get; set; }
    }

    private static readonly Dictionary<int, GameInfo> TestGames = new Dictionary<int, GameInfo>
    {
        {
            1,
            new GameInfo
            {
                Date = new DateOnly(2022, 4, 27),
                ParkNumber = 1,
                Name = "2022 Test Game 1",
                HomeScore = 0,
                AwayScore = 2,
                Away = new BoxScoreInfo
                {
                    TeamNumber = 1,
                    TeamName = "Test City Old Timers",
                    Batters = [
                        new BatterInfo
                        {
                            BatterNumber = 1,
                            PlateAppearances = 3,
                            AtBats = 3,
                            Hits = 1,
                            Homeruns = 0,
                            Runs = 2
                        }
                    ],
                    Pitchers = [
                        new PitcherInfo
                        {
                            PitcherNumber = 1,
                            Outs = 27,
                            EarnedRuns = 0,
                            Runs = 0
                        }
                    ],
                    Fielders = [
                        new FielderInfo
                        {
                            BatterNumber = 1,
                            Assists = 1,
                            Putouts = 5,
                            Errors = 0
                        },
                        new FielderInfo
                        {
                            PitcherNumber = 1,
                            Assists = 5,
                            Putouts = 1,
                            Errors = 1
                        }
                    ]
                },
                Home = new BoxScoreInfo
                {
                    TeamNumber = 2,
                    TeamName = "New Tester Town Tubes",
                    Batters = [
                        new BatterInfo
                        {
                            BatterNumber = 3,
                            PlateAppearances = 4,
                            AtBats = 4,
                            Hits = 0,
                            Homeruns = 0,
                            Runs = 0
                        }
                    ],
                    Pitchers = [
                        new PitcherInfo
                        {
                            PitcherNumber = 2,
                            Outs = 9,
                            EarnedRuns = 1,
                            Runs = 2
                        }
                    ],
                    Fielders = []
                }
            }
        },
        {
            2,
            new GameInfo
            {
                Date = new DateOnly(2023, 6, 27),
                ParkNumber = 2,
                Name = "2023 Test Game 1",
                HomeScore = 2,
                AwayScore = 3,
                Away = new BoxScoreInfo
                {
                    TeamNumber = 1,
                    Batters = [
                        new BatterInfo
                        {
                            BatterNumber = 1,
                            PlateAppearances = 4,
                            AtBats = 3,
                            Hits = 1,
                            Homeruns = 0,
                            Runs = 3
                        }
                    ],
                    Pitchers = [
                        new PitcherInfo
                        {
                            PitcherNumber = 1,
                            Outs = 15,
                            EarnedRuns = 0,
                            Runs = 2
                        }
                    ],
                    Fielders = [
                        new FielderInfo
                        {
                            BatterNumber = 1,
                            Assists = 1,
                            Putouts = 5,
                            Errors = 0
                        },
                        new FielderInfo
                        {
                            PitcherNumber = 1,
                            Assists = 5,
                            Putouts = 1,
                            Errors = 1
                        }
                    ]
                },
                Home = new BoxScoreInfo
                {
                    TeamNumber = 2,
                    Batters = [
                        new BatterInfo
                        {
                            BatterNumber = 3,
                            PlateAppearances = 3,
                            AtBats = 3,
                            Hits = 1,
                            Homeruns = 1,
                            Runs = 2
                        }
                    ],
                    Pitchers = [
                        new PitcherInfo
                        {
                            PitcherNumber = 2,
                            Outs = 14,
                            EarnedRuns = 3,
                            Runs = 3
                        }
                    ],
                    Fielders = []
                }
            }
        },
        {
            3,
            new GameInfo
            {
                Date = new DateOnly(2022, 8, 27),
                ParkNumber = 1,
                Name = "2022 Test Game 2",
                HomeScore = 0,
                AwayScore = 0,
                Away =new BoxScoreInfo
                {
                    TeamNumber = 2,
                    Batters = [
                        new BatterInfo
                        {
                            BatterNumber = 3,
                            PlateAppearances = 4,
                            AtBats = 4,
                            Hits = 2,
                            Homeruns = 1,
                            Runs = 0
                        }
                    ],
                    Pitchers = [
                        new PitcherInfo
                        {
                            PitcherNumber = 2,
                            Outs = 14,
                            EarnedRuns = 0,
                            Runs = 0
                        }
                    ],
                    Fielders = []
                },
                Home = new BoxScoreInfo
                {
                    TeamNumber = 1,
                    Batters = [
                        new BatterInfo
                        {
                            BatterNumber = 2,
                            PlateAppearances = 3,
                            AtBats = 3,
                            Hits = 1,
                            Homeruns = 1,
                            Runs = 0
                        }
                    ],
                    Pitchers = [
                        new PitcherInfo
                        {
                            PitcherNumber = 1,
                            Outs = 10,
                            EarnedRuns = 0,
                            Runs = 0
                        }
                    ],
                    Fielders = []
                }
            }
        },
        {
            4,
            new GameInfo
            {
                Date = new DateOnly(2024, 6, 30),
                ParkNumber = 1,
                Name = "2024 Test Game 1",
                AwayScore = 3,
                HomeScore = 4,
                Away = new BoxScoreInfo
                {
                    TeamNumber = 1,
                    Batters = [],
                    Pitchers = [],
                    Fielders = []
                },
                Home = new BoxScoreInfo
                {
                    TeamNumber = 3,
                    Batters = [],
                    Pitchers = [],
                    Fielders = []
                }
            }
        },
        {
            5,
            new GameInfo
            {
                Date = new DateOnly(2025, 4, 30),
                ParkNumber = 1,
                Name = "2025 Test Game 1",
                AwayScore = 1,
                HomeScore = 0,
                Away = new BoxScoreInfo
                {
                    TeamNumber = 2,
                    Batters = [],
                    Pitchers = [],
                    Fielders = []
                },
                Home = new BoxScoreInfo
                {
                    TeamNumber = 1,
                    Batters = [],
                    Pitchers = [],
                    Fielders = []
                }
            }
        },
        {
            6,
            new GameInfo
            {
                Date = new DateOnly(2025, 5, 30),
                ParkNumber = 1,
                Name = "2025 Test Game 2",
                AwayScore = 1,
                HomeScore = 0,
                Away = new BoxScoreInfo
                {
                    TeamNumber = 2,
                    Batters = [],
                    Pitchers = [],
                    Fielders = []
                },
                Home = new BoxScoreInfo
                {
                    TeamNumber = 1,
                    Batters = [],
                    Pitchers = [],
                    Fielders = []
                }
            }
        }
    };
}
