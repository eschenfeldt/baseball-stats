using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Parks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    MiddleName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Suffix = table.Column<string>(type: "text", nullable: true),
                    FangraphsPage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlternateParkNames",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ParkId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlternateParkNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlternateParkNames_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    City = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    HomeParkId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Parks_HomeParkId",
                        column: x => x.HomeParkId,
                        principalTable: "Parks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AlternateTeamNames",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    TeamId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlternateTeamNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlternateTeamNames_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    GameType = table.Column<int>(type: "integer", nullable: false),
                    HomeId = table.Column<long>(type: "bigint", nullable: false),
                    AwayId = table.Column<long>(type: "bigint", nullable: false),
                    ScheduledTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LocationId = table.Column<long>(type: "bigint", nullable: true),
                    HomeScore = table.Column<int>(type: "integer", nullable: true),
                    AwayScore = table.Column<int>(type: "integer", nullable: true),
                    WinningTeamId = table.Column<long>(type: "bigint", nullable: true),
                    WinningPitcherId = table.Column<long>(type: "bigint", nullable: true),
                    LosingPitcherId = table.Column<long>(type: "bigint", nullable: true),
                    SavingPitcherId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_Parks_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Parks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Games_Players_LosingPitcherId",
                        column: x => x.LosingPitcherId,
                        principalTable: "Players",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Games_Players_SavingPitcherId",
                        column: x => x.SavingPitcherId,
                        principalTable: "Players",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Games_Players_WinningPitcherId",
                        column: x => x.WinningPitcherId,
                        principalTable: "Players",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Games_Teams_AwayId",
                        column: x => x.AwayId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Games_Teams_HomeId",
                        column: x => x.HomeId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Games_Teams_WinningTeamId",
                        column: x => x.WinningTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BoxScores",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoxScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoxScores_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Batters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BoxScoreId = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Games = table.Column<int>(type: "integer", nullable: false),
                    PlateAppearances = table.Column<int>(type: "integer", nullable: false),
                    AtBats = table.Column<int>(type: "integer", nullable: false),
                    Runs = table.Column<int>(type: "integer", nullable: false),
                    Hits = table.Column<int>(type: "integer", nullable: false),
                    BuntSingles = table.Column<int>(type: "integer", nullable: false),
                    Singles = table.Column<int>(type: "integer", nullable: false),
                    Doubles = table.Column<int>(type: "integer", nullable: false),
                    Triples = table.Column<int>(type: "integer", nullable: false),
                    Homeruns = table.Column<int>(type: "integer", nullable: false),
                    RunsBattedIn = table.Column<int>(type: "integer", nullable: false),
                    Walks = table.Column<int>(type: "integer", nullable: false),
                    Strikeouts = table.Column<int>(type: "integer", nullable: false),
                    StrikeoutsCalled = table.Column<int>(type: "integer", nullable: false),
                    StrikeoutsSwinging = table.Column<int>(type: "integer", nullable: false),
                    HitByPitch = table.Column<int>(type: "integer", nullable: false),
                    StolenBases = table.Column<int>(type: "integer", nullable: false),
                    CaughtStealing = table.Column<int>(type: "integer", nullable: false),
                    SacrificeBunts = table.Column<int>(type: "integer", nullable: false),
                    SacrificeFlies = table.Column<int>(type: "integer", nullable: false),
                    Sacrifices = table.Column<int>(type: "integer", nullable: false),
                    ReachedOnError = table.Column<int>(type: "integer", nullable: false),
                    FieldersChoices = table.Column<int>(type: "integer", nullable: false),
                    CatchersInterference = table.Column<int>(type: "integer", nullable: false),
                    GroundedIntoDoublePlay = table.Column<int>(type: "integer", nullable: false),
                    GroundedIntoTriplePlay = table.Column<int>(type: "integer", nullable: false),
                    AtBatsWithRunnersInScoringPosition = table.Column<int>(type: "integer", nullable: false),
                    HitsWithRunnersInScoringPosition = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Batters_BoxScores_BoxScoreId",
                        column: x => x.BoxScoreId,
                        principalTable: "BoxScores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Batters_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fielders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BoxScoreId = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Games = table.Column<int>(type: "integer", nullable: false),
                    Errors = table.Column<int>(type: "integer", nullable: false),
                    ErrorsThrowing = table.Column<int>(type: "integer", nullable: false),
                    ErrorsFielding = table.Column<int>(type: "integer", nullable: false),
                    Putouts = table.Column<int>(type: "integer", nullable: false),
                    Assists = table.Column<int>(type: "integer", nullable: false),
                    StolenBaseAttempts = table.Column<int>(type: "integer", nullable: false),
                    CaughtStealing = table.Column<int>(type: "integer", nullable: false),
                    DoublePlays = table.Column<int>(type: "integer", nullable: false),
                    TriplePlays = table.Column<int>(type: "integer", nullable: false),
                    PassedBalls = table.Column<int>(type: "integer", nullable: false),
                    PickoffFailed = table.Column<int>(type: "integer", nullable: false),
                    PickoffSuccess = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fielders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fielders_BoxScores_BoxScoreId",
                        column: x => x.BoxScoreId,
                        principalTable: "BoxScores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fielders_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pitchers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BoxScoreId = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Games = table.Column<int>(type: "integer", nullable: false),
                    Wins = table.Column<int>(type: "integer", nullable: false),
                    Losses = table.Column<int>(type: "integer", nullable: false),
                    Saves = table.Column<int>(type: "integer", nullable: false),
                    ThirdInningsPitched = table.Column<int>(type: "integer", nullable: false),
                    BattersFaced = table.Column<int>(type: "integer", nullable: false),
                    Balls = table.Column<int>(type: "integer", nullable: false),
                    Strikes = table.Column<int>(type: "integer", nullable: false),
                    Pitches = table.Column<int>(type: "integer", nullable: false),
                    Runs = table.Column<int>(type: "integer", nullable: false),
                    EarnedRuns = table.Column<int>(type: "integer", nullable: false),
                    Hits = table.Column<int>(type: "integer", nullable: false),
                    Walks = table.Column<int>(type: "integer", nullable: false),
                    IntentionalWalks = table.Column<int>(type: "integer", nullable: false),
                    Strikeouts = table.Column<int>(type: "integer", nullable: false),
                    StrikeoutsCalled = table.Column<int>(type: "integer", nullable: false),
                    StrikeoutsSwinging = table.Column<int>(type: "integer", nullable: false),
                    HitByPitch = table.Column<int>(type: "integer", nullable: false),
                    Balks = table.Column<int>(type: "integer", nullable: false),
                    WildPitches = table.Column<int>(type: "integer", nullable: false),
                    Homeruns = table.Column<int>(type: "integer", nullable: false),
                    GroundOuts = table.Column<int>(type: "integer", nullable: false),
                    AirOuts = table.Column<int>(type: "integer", nullable: false),
                    FirstPitchStrikes = table.Column<int>(type: "integer", nullable: false),
                    FirstPitchBalls = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pitchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pitchers_BoxScores_BoxScoreId",
                        column: x => x.BoxScoreId,
                        principalTable: "BoxScores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pitchers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlternateParkNames_ParkId",
                table: "AlternateParkNames",
                column: "ParkId");

            migrationBuilder.CreateIndex(
                name: "IX_AlternateTeamNames_TeamId",
                table: "AlternateTeamNames",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Batters_BoxScoreId",
                table: "Batters",
                column: "BoxScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Batters_PlayerId",
                table: "Batters",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_BoxScores_GameId",
                table: "BoxScores",
                column: "GameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fielders_BoxScoreId",
                table: "Fielders",
                column: "BoxScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Fielders_PlayerId",
                table: "Fielders",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_AwayId",
                table: "Games",
                column: "AwayId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_HomeId",
                table: "Games",
                column: "HomeId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_LocationId",
                table: "Games",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_LosingPitcherId",
                table: "Games",
                column: "LosingPitcherId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_SavingPitcherId",
                table: "Games",
                column: "SavingPitcherId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_WinningPitcherId",
                table: "Games",
                column: "WinningPitcherId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_WinningTeamId",
                table: "Games",
                column: "WinningTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Pitchers_BoxScoreId",
                table: "Pitchers",
                column: "BoxScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Pitchers_PlayerId",
                table: "Pitchers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_HomeParkId",
                table: "Teams",
                column: "HomeParkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlternateParkNames");

            migrationBuilder.DropTable(
                name: "AlternateTeamNames");

            migrationBuilder.DropTable(
                name: "Batters");

            migrationBuilder.DropTable(
                name: "Fielders");

            migrationBuilder.DropTable(
                name: "Pitchers");

            migrationBuilder.DropTable(
                name: "BoxScores");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Parks");
        }
    }
}
