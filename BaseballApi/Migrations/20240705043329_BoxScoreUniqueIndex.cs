using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseballApi.Migrations
{
    /// <inheritdoc />
    public partial class BoxScoreUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BoxScores_GameId",
                table: "BoxScores");

            migrationBuilder.CreateIndex(
                name: "IX_BoxScores_GameId_TeamId",
                table: "BoxScores",
                columns: new[] { "GameId", "TeamId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BoxScores_GameId_TeamId",
                table: "BoxScores");

            migrationBuilder.CreateIndex(
                name: "IX_BoxScores_GameId",
                table: "BoxScores",
                column: "GameId");
        }
    }
}
