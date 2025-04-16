using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class BoxScoreTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoxScores_Games_GameId",
                table: "BoxScores");

            migrationBuilder.DropIndex(
                name: "IX_BoxScores_GameId",
                table: "BoxScores");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "BoxScores",
                newName: "TeamId");

            migrationBuilder.AddColumn<long>(
                name: "AwayBoxScoreId",
                table: "Games",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AwayTeamName",
                table: "Games",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "HomeBoxScoreId",
                table: "Games",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeTeamName",
                table: "Games",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Games_AwayBoxScoreId",
                table: "Games",
                column: "AwayBoxScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_HomeBoxScoreId",
                table: "Games",
                column: "HomeBoxScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_BoxScores_TeamId",
                table: "BoxScores",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_BoxScores_Teams_TeamId",
                table: "BoxScores",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_BoxScores_AwayBoxScoreId",
                table: "Games",
                column: "AwayBoxScoreId",
                principalTable: "BoxScores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_BoxScores_HomeBoxScoreId",
                table: "Games",
                column: "HomeBoxScoreId",
                principalTable: "BoxScores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoxScores_Teams_TeamId",
                table: "BoxScores");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_BoxScores_AwayBoxScoreId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_BoxScores_HomeBoxScoreId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_AwayBoxScoreId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_HomeBoxScoreId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_BoxScores_TeamId",
                table: "BoxScores");

            migrationBuilder.DropColumn(
                name: "AwayBoxScoreId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "AwayTeamName",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "HomeBoxScoreId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "HomeTeamName",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "TeamId",
                table: "BoxScores",
                newName: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_BoxScores_GameId",
                table: "BoxScores",
                column: "GameId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BoxScores_Games_GameId",
                table: "BoxScores",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
