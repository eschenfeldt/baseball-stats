using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class PlayerGameUniqueIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Pitchers_BoxScoreId_PlayerId",
                table: "Pitchers",
                columns: new[] { "BoxScoreId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fielders_BoxScoreId_PlayerId",
                table: "Fielders",
                columns: new[] { "BoxScoreId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoxScores_GameId",
                table: "BoxScores",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Batters_BoxScoreId_PlayerId",
                table: "Batters",
                columns: new[] { "BoxScoreId", "PlayerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pitchers_BoxScoreId_PlayerId",
                table: "Pitchers");

            migrationBuilder.DropIndex(
                name: "IX_Fielders_BoxScoreId_PlayerId",
                table: "Fielders");

            migrationBuilder.DropIndex(
                name: "IX_BoxScores_GameId",
                table: "BoxScores");

            migrationBuilder.DropIndex(
                name: "IX_Batters_BoxScoreId_PlayerId",
                table: "Batters");
        }
    }
}
